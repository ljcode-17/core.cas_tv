import { useEffect, useState, useRef, useCallback } from "react";
import { useSearchParams } from "react-router-dom";

/* ─── Auth helpers ──────────────────────────────────────────────────────────── */
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
  const parts = filePath.split("/");
  const fileName = parts.pop();
  const encodedPath = [...parts, encodeURIComponent(fileName)].join("/");
  return import.meta.env.DEV ? `http://localhost:5002${encodedPath}` : encodedPath;
};

/* ─── Presentation page ─────────────────────────────────────────────────────── */
const HIDE_DELAY_MS = 3000; // controls hide after 3 s of no interaction

export default function PresentationPage() {
  const [playlist, setPlaylist]           = useState([]);
  const [currentIndex, setCurrentIndex]   = useState(0);
  const [loading, setLoading]             = useState(true);
  const [error, setError]                 = useState("");
  const [playbackErrorMsg, setPlaybackErrorMsg] = useState("");

  const [searchParams] = useSearchParams();

  // Custom controls state
  const [isPaused, setIsPaused]           = useState(false);
  const [progress, setProgress]           = useState(0);   // 0–100
  const [duration, setDuration]           = useState(0);
  const [volume, setVolume]               = useState(1);
  const [controlsVisible, setControlsVisible] = useState(false);

  const videoRef      = useRef(null);
  const wrapperRef    = useRef(null);
  const hideTimerRef  = useRef(null);

  /* ── Fetch playlist ────────────────────────────────────────────────────────── */
  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const res = await authFetch("/api/videos");
        if (!res.ok) throw new Error("Failed to fetch videos");
        const json = await res.json();
        const allVideos = json.result || json.videos || [];
        
        const queueParam = searchParams.get("queue");
        let list;

        if (queueParam) {
          // If queue exists, use exactly those IDs in that order, provided they are Active
          const ids = queueParam.split(",").map((id) => parseInt(id, 10));
          list = ids
            .map((id) => allVideos.find((v) => v.id === id && v.status === "Active"))
            .filter(Boolean);
        } else {
          // Otherwise play all Active videos, sorted by creation order
          list = allVideos
            .filter((v) => v.status === "Active")
            .sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        }

        setPlaylist(list);
        setCurrentIndex(0);
      } catch (e) {
        setError(e.message || "Failed to load videos");
      } finally {
        setLoading(false);
      }
    })();
  }, [searchParams]);

  const current = playlist[currentIndex];

  /* ── Load + play whenever current video changes ────────────────────────────── */
  useEffect(() => {
    const vid = videoRef.current;
    if (!vid || !current) return;

    setPlaybackErrorMsg("");
    setProgress(0);
    setDuration(0);
    setIsPaused(false);

    // Set src imperatively so the browser doesn't flash native controls on reload.
    // Adding `current` to the dep array ensures this fires when the playlist
    // first loads (currentIndex stays 0 but current goes undefined → playlist[0]).
    vid.src = getVideoSrc(current.filePath);
    const playPromise = vid.play();
    if (playPromise !== undefined) {
      playPromise.catch((err) =>
        console.warn("Autoplay blocked or failed:", err.message)
      );
    }
  }, [currentIndex, current]); // `current` is intentional — triggers on first playlist load

  /* ── Controls idle-hide logic ──────────────────────────────────────────────── */
  const showControlsTemporarily = useCallback(() => {
    setControlsVisible(true);
    clearTimeout(hideTimerRef.current);
    hideTimerRef.current = setTimeout(() => setControlsVisible(false), HIDE_DELAY_MS);
  }, []);

  useEffect(() => () => clearTimeout(hideTimerRef.current), []);

  /* ── Video event handlers ──────────────────────────────────────────────────── */
  const handleEnded = () => {
    if (!playlist.length) return;
    setCurrentIndex((prev) => (prev + 1) % playlist.length);
  };

  const handleError = () => {
    setPlaybackErrorMsg(
      `Cannot play "${current?.fileName}". Format not supported. Skipping...`
    );
    setTimeout(() => {
      setPlaybackErrorMsg("");
      setCurrentIndex((prev) => (prev + 1) % playlist.length);
    }, 4000);
  };

  const handleTimeUpdate = () => {
    const vid = videoRef.current;
    if (!vid || !vid.duration) return;
    setProgress((vid.currentTime / vid.duration) * 100);
  };

  const handleLoadedMetadata = () => {
    const vid = videoRef.current;
    if (vid) setDuration(vid.duration);
  };

  const handlePauseEvent  = () => setIsPaused(true);
  const handlePlayEvent   = () => setIsPaused(false);

  /* ── Custom control actions ────────────────────────────────────────────────── */
  const togglePlay = (e) => {
    e.stopPropagation();
    const vid = videoRef.current;
    if (!vid) return;
    if (vid.paused) { vid.play(); setIsPaused(false); }
    else            { vid.pause(); setIsPaused(true); }
    showControlsTemporarily();
  };

  const handleSeek = (e) => {
    e.stopPropagation();
    const vid = videoRef.current;
    if (!vid || !vid.duration) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const ratio = (e.clientX - rect.left) / rect.width;
    vid.currentTime = ratio * vid.duration;
    showControlsTemporarily();
  };

  const handleVolume = (e) => {
    e.stopPropagation();
    const val = parseFloat(e.target.value);
    setVolume(val);
    if (videoRef.current) videoRef.current.volume = val;
    showControlsTemporarily();
  };

  const handleFullscreen = (e) => {
    e.stopPropagation();
    const el = wrapperRef.current;
    if (!el) return;
    if (!document.fullscreenElement) el.requestFullscreen?.();
    else document.exitFullscreen?.();
    showControlsTemporarily();
  };

  const fmtTime = (s) => {
    if (!s || isNaN(s)) return "0:00";
    const m = Math.floor(s / 60);
    const sec = Math.floor(s % 60).toString().padStart(2, "0");
    return `${m}:${sec}`;
  };

  /* ── State screens ─────────────────────────────────────────────────────────── */
  if (loading) return (
    <div className="w-full h-screen bg-black flex items-center justify-center">
      <div className="text-white text-[16px] animate-pulse">Loading presentation…</div>
    </div>
  );

  if (error) return (
    <div className="w-full h-screen bg-black flex items-center justify-center">
      <div className="text-red-400 text-[16px]">{error}</div>
    </div>
  );

  if (playlist.length === 0) return (
    <div className="w-full h-screen bg-black flex items-center justify-center">
      <div className="text-white text-[16px]">No active videos available.</div>
    </div>
  );

  /* ── Render ────────────────────────────────────────────────────────────────── */
  return (
    <div
      ref={wrapperRef}
      className="w-full h-screen bg-black overflow-hidden relative select-none"
      onMouseMove={showControlsTemporarily}
      onMouseLeave={() => {
        clearTimeout(hideTimerRef.current);
        setControlsVisible(false);
      }}
      onClick={showControlsTemporarily}
      style={{ cursor: controlsVisible ? "default" : "none" }}
    >
      {/* ── Video element — NO native controls attribute ── */}
      <video
        ref={videoRef}
        autoPlay
        preload="auto"
        playsInline
        onEnded={handleEnded}
        onError={handleError}
        onTimeUpdate={handleTimeUpdate}
        onLoadedMetadata={handleLoadedMetadata}
        onPause={handlePauseEvent}
        onPlay={handlePlayEvent}
        className="w-full h-full object-contain bg-black"
      />

      {/* ── Custom controls overlay — only visible on interaction ── */}
      <div
        className="absolute bottom-0 left-0 right-0 z-20 transition-opacity duration-300"
        style={{ opacity: controlsVisible ? 1 : 0, pointerEvents: controlsVisible ? "auto" : "none" }}
      >
        {/* Gradient fade so controls are readable over any video */}
        <div className="bg-gradient-to-t from-black/80 via-black/40 to-transparent px-5 pt-8 pb-4">

          {/* Seek bar */}
          <div
            className="w-full h-[4px] bg-white/30 rounded-full mb-3 cursor-pointer relative group"
            onClick={handleSeek}
          >
            <div
              className="h-full bg-white rounded-full pointer-events-none"
              style={{ width: `${progress}%` }}
            />
            {/* Thumb dot */}
            <div
              className="absolute top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full shadow pointer-events-none"
              style={{ left: `calc(${progress}% - 6px)` }}
            />
          </div>

          {/* Bottom row */}
          <div className="flex items-center gap-4">
            {/* Play / Pause */}
            <button
              onClick={togglePlay}
              className="text-white hover:text-white/80 transition-colors w-8 h-8 flex items-center justify-center"
              title={isPaused ? "Play" : "Pause"}
            >
              {isPaused ? (
                /* Play triangle */
                <svg viewBox="0 0 24 24" fill="currentColor" className="w-6 h-6">
                  <path d="M8 5v14l11-7z" />
                </svg>
              ) : (
                /* Pause bars */
                <svg viewBox="0 0 24 24" fill="currentColor" className="w-6 h-6">
                  <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z" />
                </svg>
              )}
            </button>

            {/* Time */}
            <span className="text-white/70 text-[12px] tabular-nums">
              {fmtTime(duration * (progress / 100))} / {fmtTime(duration)}
            </span>

            {/* Spacer */}
            <div className="flex-1" />

            {/* Volume */}
            <div className="flex items-center gap-2">
              <svg viewBox="0 0 24 24" fill="currentColor" className="w-5 h-5 text-white/70 shrink-0">
                <path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02z" />
              </svg>
              <input
                type="range"
                min={0}
                max={1}
                step={0.05}
                value={volume}
                onChange={handleVolume}
                onClick={(e) => e.stopPropagation()}
                className="w-20 accent-white cursor-pointer"
              />
            </div>

            {/* Counter */}
            <span className="text-white/50 text-[12px]">
              {currentIndex + 1} / {playlist.length}
            </span>

            {/* Fullscreen */}
            <button
              onClick={handleFullscreen}
              className="text-white/70 hover:text-white transition-colors w-7 h-7 flex items-center justify-center"
              title="Fullscreen"
            >
              <svg viewBox="0 0 24 24" fill="currentColor" className="w-5 h-5">
                <path d="M7 14H5v5h5v-2H7v-3zm-2-4h2V7h3V5H5v5zm12 7h-3v2h5v-5h-2v3zM14 5v2h3v3h2V5h-5z" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      {/* ── Codec error overlay ── */}
      {playbackErrorMsg && (
        <div className="absolute inset-0 flex items-center justify-center p-4 z-30">
          <div className="bg-red-900/80 text-white px-6 py-4 rounded-lg text-center max-w-lg border border-red-500/50 shadow-2xl backdrop-blur-sm">
            <span className="block text-2xl mb-2">⚠️</span>
            <p className="text-[15px] font-medium">{playbackErrorMsg}</p>
          </div>
        </div>
      )}
    </div>
  );
}
