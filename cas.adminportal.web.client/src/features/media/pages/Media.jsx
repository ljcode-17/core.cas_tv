import { useEffect, useState, useRef } from "react";
import { useSearchParams } from "react-router-dom";

const isTokenExpired = (token) => {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
};

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

const getVideoSrc = (filePath) => {
  if (!filePath) return "";
  
  // Safely encode the filename to handle special characters like #, &, +, etc.
  // which would otherwise break the URL.
  const parts = filePath.split("/");
  const fileName = parts.pop();
  const encodedPath = [...parts, encodeURIComponent(fileName)].join("/");
  
  return import.meta.env.DEV ? `http://localhost:5002${encodedPath}` : encodedPath;
};

export default function MediaPage() {
  const [searchParams] = useSearchParams();
  const [playlist, setPlaylist] = useState([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [playbackErrorMsg, setPlaybackErrorMsg] = useState("");
  const [isBuffering, setIsBuffering] = useState(false);

  const videoRef = useRef(null);

  // ── Load playlist on mount ────────────────────────────────────────────────
  useEffect(() => {
    const fetchVideos = async () => {
      setLoading(true);
      try {
        const res = await authFetch("/api/videos");
        if (!res.ok) throw new Error("Failed to fetch videos");
        const json = await res.json();
        const allVideos = json.result || json.videos || [];

        const queueParam = searchParams.get("queue");
        let list;

        if (queueParam) {
          const ids = queueParam.split(",").map(id => parseInt(id, 10));
          list = ids
            .map(id => allVideos.find(v => v.id === id && v.status === "Active"))
            .filter(Boolean);
        } else {
          list = allVideos
            .filter(v => v.status === "Active")
            .sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        }

        setPlaylist(list);
        setCurrentIndex(0);
      } catch (e) {
        console.error(e);
        setError(e.message || "Failed to load videos");
      } finally {
        setLoading(false);
      }
    };

    fetchVideos();
  }, [searchParams]);

  const current = playlist[currentIndex];

  // ── Auto-play whenever the current video changes ──────────────────────────
  useEffect(() => {
    if (videoRef.current && current) {
      setPlaybackErrorMsg("");
      setIsBuffering(false);
      
      // Force the browser to load the new src and play without destroying the video element.
      // This preserves native Fullscreen mode across different videos!
      videoRef.current.load();
      
      const playPromise = videoRef.current.play();
      if (playPromise !== undefined) {
        playPromise.catch((err) => {
          console.warn("Autoplay blocked or failed:", err.message);
        });
      }
    }
  }, [currentIndex, current]);

  // ── Handlers ──────────────────────────────────────────────────────────────
  const handleEnded = () => {
    if (!playlist.length) return;
    setCurrentIndex((prev) => (prev + 1) % playlist.length);
  };

  const handleError = (e) => {
    console.error("Video element error:", e);
    setPlaybackErrorMsg(`Cannot play "${current?.fileName}". The file format or codec is not supported. Skipping...`);
    setIsBuffering(false);
    setTimeout(() => {
      setPlaybackErrorMsg("");
      setCurrentIndex((prev) => (prev + 1) % playlist.length);
    }, 4000);
  };

  const handleWaiting = () => setIsBuffering(true);
  const handlePlaying = () => setIsBuffering(false);

  if (loading) {
    return (
      <div className="w-full h-screen bg-black flex items-center justify-center">
        <div className="text-white text-[16px] animate-pulse">Loading playlist…</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="w-full h-screen bg-black flex items-center justify-center">
        <div className="text-red-400 text-[16px]">{error}</div>
      </div>
    );
  }

  if (playlist.length === 0) {
    return (
      <div className="w-full h-screen bg-black flex items-center justify-center">
        <div className="text-white text-[16px]">No active videos available.</div>
      </div>
    );
  }

  return (
    <div className="w-full h-screen bg-black flex flex-col items-center justify-center">
      {/* Info bar */}
      <div className="w-full max-w-[1400px] px-4 mb-2 flex items-center justify-between">
        <div className="text-white/70 text-[13px] flex items-center gap-3">
          <span>
            <span className="text-white font-semibold">{currentIndex + 1}</span> / {playlist.length}
          </span>
          <span className="text-white/50 text-[12px] truncate max-w-[600px]">
            {current?.fileName}
          </span>
          {isBuffering && (
            <span className="text-blue-400 text-[12px] animate-pulse flex items-center gap-1">
              <span className="w-2 h-2 bg-blue-500 rounded-full inline-block"></span> Buffering from server...
            </span>
          )}
        </div>
        <div className="text-white/40 text-[12px] italic">Loops automatically ↺</div>
      </div>

      {/* Video container */}
      <div className="w-full max-w-[1400px] px-4 relative">
        <video
          ref={videoRef}
          src={getVideoSrc(current?.filePath)}
          controls
          autoPlay
          preload="auto"
          playsInline
          onEnded={handleEnded}
          onError={handleError}
          onWaiting={handleWaiting}
          onPlaying={handlePlaying}
          className={`w-full h-[85vh] object-contain rounded-lg bg-black transition-opacity ${playbackErrorMsg ? "opacity-20" : ""} ${isBuffering ? "opacity-80" : ""}`}
        />
        
        {/* Error Overlay */}
        {playbackErrorMsg && (
          <div className="absolute inset-0 flex items-center justify-center p-4 z-10">
            <div className="bg-red-900/80 text-white px-6 py-4 rounded-lg text-center max-w-lg border border-red-500/50 shadow-2xl backdrop-blur-sm">
              <span className="block text-2xl mb-2">⚠️</span>
              <p className="text-[15px] font-medium">{playbackErrorMsg}</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
