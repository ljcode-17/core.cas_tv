import { useState, useEffect } from "react";
import { useAuthContext } from "@features/auth/useAuthContext";
import { useDataStore } from "@shared/api";
import { ArrowsClockwise, Play, Pencil, Trash, X, CheckSquare } from "@phosphor-icons/react";
import { useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle, CardContent } from "@shared/components/ui/card";
import { Button } from "@shared/components/ui/button";
import { Badge } from "@shared/components/ui/badge";
import {
  Table,
  TableHeader,
  TableRow,
  TableHead,
  TableBody,
  TableCell,
} from "@shared/components/ui/table";
import { EditVideoModal } from "../components/EditVideoModal";

/** Check if a JWT is expired */
const isTokenExpired = (token) => {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
};

/** Fetch wrapper that attaches auth headers */
const authFetch = (url, options = {}) => {
  let token = localStorage.getItem("accessToken");
  if (token && isTokenExpired(token)) {
    localStorage.removeItem("accessToken");
    token = null;
  }
  const headers = new Headers(options.headers || {});
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  } else {
    headers.set("X-Dev-UserId", "1");
    headers.set("X-Dev-UserEmail", "dev@localhost");
  }
  return fetch(url, { ...options, headers });
};

export default function Home() {
  const { user } = useAuthContext();
  const { actionSelect } = useDataStore();
  const navigate = useNavigate();

  // Videos
  const [videos, setVideos] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState("");

  // Queue: ordered list of video IDs the user has selected
  const [queuedVideos, setQueuedVideos] = useState([]);

  // Modals
  const [editingVideo, setEditingVideo] = useState(null);
  const [deletingVideo, setDeletingVideo] = useState(null);

  useEffect(() => { fetchVideos(); }, []);

  const fetchVideos = async () => {
    try {
      const res = await authFetch("/api/videos");
      if (!res.ok) throw new Error("API not available");
      const json = await res.json();
      const videoList = json.result || json.videos || [];
      const mapped = videoList.map(v => ({
        ...v,
        url: v.filePath ? v.filePath : (v.url || "")
      }));
      setVideos(mapped);
      // Drop from queue any video that is no longer Active
      setQueuedVideos(prev =>
        prev.filter(q => mapped.find(m => m.id === q.id && m.status === "Active"))
      );
      saveToLocalStorage(mapped);
    } catch {
      const stored = localStorage.getItem("core_tv_videos");
      if (stored) {
        try { setVideos(JSON.parse(stored)); } catch { /* ignore */ }
      }
    }
  };

  const saveToLocalStorage = (list) => {
    const meta = list.map(({ id, fileName, status, fileSize, url }) => ({
      id, fileName, status, fileSize,
      url: url && url.startsWith("blob:") ? "" : url,
    }));
    localStorage.setItem("core_tv_videos", JSON.stringify(meta));
  };

  // ── Upload ──────────────────────────────────────────────────────────────────
  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    if (!file.type.startsWith("video/")) {
      setUploadError("Only video files are allowed.");
      setSelectedFile(null);
      return;
    }
    setSelectedFile(file);
    setUploadError("");
  };

  const handleUpload = () => {
    if (!selectedFile) return;
    setIsUploading(true);
    setUploadError("");
    (async () => {
      try {
        const fd = new FormData();
        fd.append("file", selectedFile);
        const res = await authFetch("/api/videos/upload", { method: "POST", body: fd });
        const json = await res.json();
        if (!res.ok || !json.success) throw new Error(json.message || "Upload failed");
        await fetchVideos();
        setSelectedFile(null);
        const fi = document.getElementById("video-file-input");
        if (fi) fi.value = "";
      } catch (e) {
        setUploadError(e.message || "Upload failed");
      } finally {
        setIsUploading(false);
      }
    })();
  };

  // ── Edit ────────────────────────────────────────────────────────────────────
  const handleSaveStatus = (id, newStatus) => {
    (async () => {
      try {
        const res = await authFetch(`/api/videos/${id}`, {
          method: "PATCH",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ status: newStatus }),
        });
        if (!res.ok) throw new Error("Failed to update status");
        await fetchVideos();
        setEditingVideo(null);
      } catch (e) {
        console.error(e);
      }
    })();
  };

  // ── Remove ──────────────────────────────────────────────────────────────────
  const handleDeleteConfirm = () => {
    if (!deletingVideo) return;
    (async () => {
      try {
        const res = await authFetch(`/api/videos/${deletingVideo.id}`, { method: "DELETE" });
        if (!res.ok) throw new Error("Failed to remove");
        
        // Optimistically remove from UI immediately
        setVideos(prev => prev.filter(v => v.id !== deletingVideo.id));
        setQueuedVideos(prev => prev.filter(q => q.id !== deletingVideo.id));
        
        await fetchVideos();
        setDeletingVideo(null);
      } catch (e) {
        console.error(e);
      }
    })();
  };

  // ── Queue helpers ───────────────────────────────────────────────────────────
  const queuePosition = (id) => {
    const idx = queuedVideos.findIndex(v => v.id === id);
    return idx === -1 ? 0 : idx + 1;
  };

  const toggleQueue = (video) => {
    if (video.status !== "Active") return;
    setQueuedVideos(prev => {
      const exists = prev.find(v => v.id === video.id);
      return exists ? prev.filter(v => v.id !== video.id) : [...prev, video];
    });
  };

  const removeFromQueue = (id) => setQueuedVideos(prev => prev.filter(v => v.id !== id));
  const clearQueue = () => setQueuedVideos([]);

  const handleRefresh = async () => {
    clearQueue();
    await fetchVideos();
  };

  const handlePlay = () => {
    const playlist = queuedVideos.length > 0
      ? queuedVideos
      : videos.filter(v => v.status === "Active");
    if (playlist.length === 0) return;
    
    // Open the standalone presentation page in a new tab.
    // import.meta.env.BASE_URL = "/cas/adminportal/" (from vite.config.js)
    let url = `${import.meta.env.BASE_URL}presentation`;
    if (queuedVideos.length > 0) {
      const ids = playlist.map(v => v.id).join(",");
      url += `?queue=${ids}`;
    }
    window.open(url, "_blank");
  };

  const activeVideos = videos.filter(v => v.status === "Active");
  const playlistCount = queuedVideos.length > 0 ? queuedVideos.length : activeVideos.length;

  return (
    <div className="w-full">
      <div className="text-[#6B7280] text-[13px] mb-[28px]">CORETvApp</div>

      <Card className="rounded-[10px] shadow-none border-[#E5E7EB]">
        <CardHeader className="px-[26px] pt-[26px] pb-0 border-none">
          <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-[26px]">
            <CardTitle className="text-[28px] font-bold text-black font-serif">Settings</CardTitle>

            <div className="flex items-center gap-2 mt-4 sm:mt-0">
              {queuedVideos.length > 0 && (
                <Button
                  variant="outline"
                  onClick={clearQueue}
                  className="border-[#E5E7EB] hover:bg-gray-100 h-[40px] px-[14px] rounded-[3px] flex items-center gap-2 text-[14px] text-gray-600"
                >
                  <X size={15} />
                  Clear Queue ({queuedVideos.length})
                </Button>
              )}
              <Button
                onClick={handlePlay}
                disabled={playlistCount === 0}
                className="bg-[#EF4444] disabled:bg-red-300 text-white font-semibold h-[40px] px-[18px] rounded-[3px] flex items-center gap-2 hover:bg-red-600 transition-colors"
              >
                <Play size={18} weight="fill" />
                {queuedVideos.length > 0
                  ? `Play Queue (${queuedVideos.length})`
                  : `Play Active Videos (${activeVideos.length})`}
              </Button>
            </div>
          </div>
        </CardHeader>

        <CardContent className="px-[26px] pb-[26px]">
          {/* Upload controls */}
          <div className="flex flex-col xl:flex-row xl:justify-between xl:items-center gap-4 mb-[32px]">
            <div className="flex flex-col sm:flex-row sm:items-center gap-3">
              <label className="flex items-center justify-center cursor-pointer border border-[#E5E7EB] bg-gray-50 rounded-[3px] px-3 h-[40px] hover:bg-gray-100 transition-colors">
                <span className="text-[#374151] font-medium text-[14px]">Choose file</span>
                <input
                  id="video-file-input"
                  type="file"
                  accept="video/*"
                  onChange={handleFileChange}
                  className="hidden"
                  disabled={isUploading}
                />
              </label>
              <span className="text-[#9CA3AF] text-[14px] truncate max-w-[200px]">
                {selectedFile ? selectedFile.name : "No file chosen"}
              </span>
              {selectedFile && (
                <Button
                  onClick={handleUpload}
                  disabled={isUploading}
                  className="bg-[#2563EB] text-white font-semibold h-[40px] px-4 rounded-[3px] hover:bg-blue-700 transition-colors disabled:opacity-50"
                >
                  {isUploading ? "Uploading..." : "Upload Video"}
                </Button>
              )}
            </div>
            <div className="flex items-center gap-3">
              <Button
                onClick={handleRefresh}
                className="bg-[#2563EB] text-white font-semibold h-[40px] px-[18px] rounded-[3px] flex items-center gap-2 hover:bg-blue-700 transition-colors"
              >
                <ArrowsClockwise size={18} weight="bold" />
                Refresh
              </Button>
              <span className="text-[#8B95A1] text-[14px]">Select a video to upload</span>
            </div>
          </div>

          {uploadError && (
            <div className="text-red-500 text-[14px] mb-4 font-medium">{uploadError}</div>
          )}

          {/* Queue hint */}
          {activeVideos.length > 0 && (
            <div className="flex items-center gap-2 mb-3 text-[13px] text-[#6B7280] bg-blue-50 border border-blue-100 rounded-[6px] px-3 py-2">
              <CheckSquare size={16} className="text-blue-500 shrink-0" />
              <span>
                <strong className="text-blue-700">Build a custom queue:</strong> Click the checkbox next to any{" "}
                <span className="font-semibold text-green-700">Active</span> video to add it to your playlist in that order.
                Leave all unchecked to play all active videos in default order.
              </span>
            </div>
          )}

          {/* Table */}
          <div className="w-full overflow-x-auto">
            <Table className="w-full min-w-[640px] text-left border-collapse">
              <TableHeader>
                <TableRow className="border-b border-[#E5E7EB] hover:bg-transparent">
                  <TableHead className="w-[5%] pb-3"></TableHead>
                  <TableHead className="w-[8%] pb-3 font-bold text-black text-[14px]">#</TableHead>
                  <TableHead className="w-[47%] pb-3 font-bold text-black text-[14px]">File Name</TableHead>
                  <TableHead className="w-[15%] pb-3 font-bold text-black text-[14px]">Status</TableHead>
                  <TableHead className="w-[25%] pb-3 font-bold text-black text-[14px]">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {videos.map((video, index) => {
                  const pos = queuePosition(video.id);
                  const isQueued = pos > 0;
                  const isActive = video.status === "Active";
                  return (
                    <TableRow
                      key={video.id}
                      className={`border-b border-[#F3F4F6] transition-colors ${isQueued ? "bg-blue-50/60 hover:bg-blue-50" : "hover:bg-gray-50/50"}`}
                    >
                      <TableCell className="py-3.5 text-center">
                        {isActive ? (
                          <button
                            onClick={() => toggleQueue(video)}
                            title={isQueued ? `Remove from queue (position ${pos})` : "Add to queue"}
                            className={`w-6 h-6 rounded-[4px] border-2 flex items-center justify-center transition-all mx-auto
                              ${isQueued ? "bg-blue-600 border-blue-600 text-white shadow-sm" : "border-gray-300 hover:border-blue-400 bg-white"}`}
                          >
                            {isQueued && <span className="text-[11px] font-bold leading-none">{pos}</span>}
                          </button>
                        ) : (
                          <div className="w-6 h-6 rounded-[4px] border-2 border-gray-200 bg-gray-50 mx-auto" title="Only Active videos can be queued" />
                        )}
                      </TableCell>
                      <TableCell className="py-3.5 text-[14px] text-gray-700">{index + 1}</TableCell>
                      <TableCell className="py-3.5 text-[14px] font-medium text-black truncate max-w-[300px]">
                        {video.fileName}
                        {isQueued && (
                          <span className="ml-2 text-[11px] font-semibold text-blue-600 bg-blue-100 px-1.5 py-0.5 rounded">
                            Queue #{pos}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="py-3.5">
                        <Badge variant={isActive ? "success" : "secondary"} appearance="light" size="sm">
                          {video.status}
                        </Badge>
                      </TableCell>
                      <TableCell className="py-3.5 flex items-center gap-2">
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => setEditingVideo(video)}
                          className="h-8 px-2.5 flex items-center gap-1 border-[#E5E7EB] hover:bg-gray-100 text-gray-700"
                        >
                          <Pencil size={14} /> Edit
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => setDeletingVideo(video)}
                          className="h-8 px-2.5 flex items-center gap-1 border-red-200 hover:bg-red-50 text-red-600"
                        >
                          <Trash size={14} /> Remove
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>

            {videos.length === 0 && (
              <div className="w-full text-center py-[48px] text-[#4B5563] text-[15px] border-b border-[#E5E7EB]">
                No videos uploaded yet. Upload a video to get started.
              </div>
            )}
          </div>

          {/* Queue summary panel */}
          {queuedVideos.length > 0 && (
            <div className="mt-6 border border-blue-200 rounded-[8px] bg-blue-50 p-4">
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-[14px] font-bold text-blue-800 flex items-center gap-2">
                  <Play size={15} weight="fill" className="text-blue-600" />
                  Playback Queue — {queuedVideos.length} video{queuedVideos.length !== 1 ? "s" : ""}
                </h4>
                <button onClick={clearQueue} className="text-[12px] text-blue-500 hover:text-blue-700 underline">
                  Clear all
                </button>
              </div>
              <ol className="space-y-1.5">
                {queuedVideos.map((v, i) => (
                  <li key={v.id} className="flex items-center gap-3 bg-white border border-blue-100 rounded-[5px] px-3 py-2 text-[13px]">
                    <span className="w-5 h-5 flex items-center justify-center bg-blue-600 text-white text-[11px] font-bold rounded-full shrink-0">
                      {i + 1}
                    </span>
                    <span className="flex-1 truncate font-medium text-gray-800">{v.fileName}</span>
                    <button onClick={() => removeFromQueue(v.id)} className="text-gray-400 hover:text-red-500 transition-colors shrink-0" title="Remove from queue">
                      <X size={14} />
                    </button>
                  </li>
                ))}
              </ol>
              <p className="text-[12px] text-blue-600 mt-3 italic">↺ After the last video, the queue loops back to the beginning.</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Modal */}
      <EditVideoModal
        video={editingVideo}
        isOpen={!!editingVideo}
        onClose={() => setEditingVideo(null)}
        onSave={async (id, newStatus) => {
          const res = await authFetch(`/api/videos/${id}`, {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ status: newStatus }),
          });
          if (!res.ok) throw new Error("Failed to update status");
          await fetchVideos();
        }}
      />

      {/* Remove Modal */}
      {deletingVideo && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 transition-all duration-300">
          <div className="bg-white rounded-[20px] shadow-2xl max-w-[420px] w-full p-8 relative transform scale-100 transition-all duration-300">
            <button 
              onClick={() => setDeletingVideo(null)} 
              className="absolute top-5 right-5 text-gray-400 hover:text-gray-700 bg-gray-50 hover:bg-gray-100 rounded-full p-2 transition-colors focus:outline-none"
            >
              <X size={18} weight="bold" />
            </button>
            
            <div className="flex flex-col items-center text-center">
              <div className="w-[68px] h-[68px] bg-red-50 text-red-500 rounded-full flex items-center justify-center mb-6 ring-[8px] ring-red-50/50">
                <Trash size={32} weight="duotone" />
              </div>
              
              <h3 className="text-[22px] font-bold text-gray-900 mb-3 font-serif">Remove Video?</h3>
              <p className="text-[15px] text-gray-500 mb-8 leading-relaxed px-1">
                Are you sure you want to remove <br/>
                <span className="font-semibold text-gray-900">"{deletingVideo.fileName}"</span>?
                <br/><br/>
                This will change its status to <span className="font-semibold text-orange-500">Inactive</span>, removing it from your playback queues.
              </p>
              
              <div className="flex w-full gap-3">
                <Button 
                  variant="outline" 
                  onClick={() => setDeletingVideo(null)} 
                  className="flex-1 border-[#E5E7EB] hover:bg-gray-50 text-gray-700 h-[48px] rounded-[10px] text-[15px] font-semibold transition-all focus:ring-2 focus:ring-gray-200 focus:outline-none"
                >
                  Cancel
                </Button>
                <Button 
                  onClick={handleDeleteConfirm} 
                  className="flex-1 bg-red-500 text-white hover:bg-red-600 h-[48px] rounded-[10px] text-[15px] font-semibold shadow-sm hover:shadow transition-all focus:ring-2 focus:ring-red-500 focus:outline-none"
                >
                  Yes, Remove
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
