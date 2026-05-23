import express from "express";
import cors from "cors";
import fs from "fs";
import fse from "fs-extra";
import path from "path";
import bodyParser from "body-parser";
import fetch from "node-fetch";
import multer from "multer";

const app = express();
app.use(cors());
app.use(bodyParser.json());

const OLLAMA_URL = "http://localhost:11434/api/generate";

const readFolder = (dirPath) => {
    let content = [];
    const entries = fs.readdirSync(dirPath, { withFileTypes: true });
    for (const entry of entries) {
        const fullPath = path.join(dirPath, entry.name);
        if (entry.isDirectory()) {
            content = content.concat(readFolder(fullPath));
        } else if (/\.(js|jsx|ts|tsx)$/.test(entry.name)) {
            const fileData = fs.readFileSync(fullPath, "utf8");
            content.push(`/** FILE: ${fullPath} **/\n${fileData}`);
        }
    }
    return content.join("\n\n");
};

// --- Video uploads storage (dev server) ---
const UPLOADS_DIR = path.join(process.cwd(), "uploads");
const VIDEOS_JSON = path.join(process.cwd(), "videos.json");
fse.ensureDirSync(UPLOADS_DIR);

const storage = multer.diskStorage({
    destination: function (req, file, cb) {
        cb(null, UPLOADS_DIR);
    },
    filename: function (req, file, cb) {
        const unique = Date.now() + "-" + file.originalname.replace(/\s+/g, "_");
        cb(null, unique);
    },
});

const upload = multer({ storage });

const readVideos = () => {
    try {
        if (!fs.existsSync(VIDEOS_JSON)) return [];
        const raw = fs.readFileSync(VIDEOS_JSON, "utf8");
        return JSON.parse(raw || "[]");
    } catch (e) {
        console.error("readVideos error", e);
        return [];
    }
};

const writeVideos = (arr) => {
    fs.writeFileSync(VIDEOS_JSON, JSON.stringify(arr, null, 2), "utf8");
};

// serve uploaded files
app.use("/uploads", express.static(UPLOADS_DIR));

// GET videos
app.get("/api/videos", (req, res) => {
    try {
        const videos = readVideos();
        return res.json({ success: true, videos });
    } catch (e) {
        return res.status(500).json({ success: false, message: e.message });
    }
});

// POST upload video
app.post("/api/videos/upload", upload.single("file"), (req, res) => {
    try {
        if (!req.file) return res.status(400).json({ success: false, message: "No file uploaded" });
        const videos = readVideos();
        const id = Date.now();
        const record = {
            id,
            fileName: req.file.originalname,
            filePath: `/uploads/${req.file.filename}`,
            status: "Active",
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
        };
        videos.unshift(record);
        writeVideos(videos);
        return res.json({ success: true, video: record });
    } catch (e) {
        console.error(e);
        return res.status(500).json({ success: false, message: e.message });
    }
});

// PATCH update status
app.patch("/api/videos/:id", (req, res) => {
    try {
        const id = Number(req.params.id);
        const { status } = req.body;
        const videos = readVideos();
        const idx = videos.findIndex(v => v.id === id);
        if (idx === -1) return res.status(404).json({ success: false, message: "Video not found" });
        videos[idx].status = status;
        videos[idx].updatedAt = new Date().toISOString();
        writeVideos(videos);
        return res.json({ success: true, video: videos[idx] });
    } catch (e) {
        return res.status(500).json({ success: false, message: e.message });
    }
});

// DELETE video
app.delete("/api/videos/:id", (req, res) => {
    try {
        const id = Number(req.params.id);
        const videos = readVideos();
        const idx = videos.findIndex(v => v.id === id);
        if (idx === -1) return res.status(404).json({ success: false, message: "Video not found" });
        const [removed] = videos.splice(idx, 1);
        // remove file if exists
        try {
            const fp = path.join(process.cwd(), removed.filePath.replace(/^\//, ""));
            if (fs.existsSync(fp)) fs.unlinkSync(fp);
        } catch (e) { console.error("failed to delete file", e); }
        writeVideos(videos);
        return res.json({ success: true });
    } catch (e) {
        return res.status(500).json({ success: false, message: e.message });
    }
});

app.post("/api/summarize-docs", async (req, res) => {
    try {
        const folderPath = path.resolve("../src/pages");
        if (!fs.existsSync(folderPath)) {
            return res.status(400).json({ error: "Folder not found" });
        }

        const folderContent = readFolder(folderPath).slice(0, 5000); // Optional: Limit characters for small models

        const fetchRes = await fetch(OLLAMA_URL, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                model: "codellama:7b-instruct",
                prompt: `Act as a senior software engineer.\n\nYou are given the source code below:\n\n${folderContent}\n\nSummarize how this system works. Focus only on functionality and important logic from the "src/pages" folder.`,
                stream: false,
            }),
        });

        const data = await fetchRes.json();
        res.json({ summary: data.response });
    } catch (err) {
        console.error(err);
        res.status(500).json({ error: "Failed to generate summary" });
    }
});

const PORT = 4000;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
