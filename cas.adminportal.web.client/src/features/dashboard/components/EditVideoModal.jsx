import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@shared/components/ui/dialog";
import { Button } from "@shared/components/ui/button";
import { Badge } from "@shared/components/ui/badge";
import { PencilSimple, VideoCamera, CheckCircle, XCircle, Check } from "@phosphor-icons/react";

export const ModalHeader = ({ title, description, icon: Icon }) => (
  <DialogHeader>
    <div className="flex items-center gap-2 mb-1">
      {Icon && <Icon size={22} className="text-blue-600" weight="duotone" />}
      <DialogTitle className="text-xl font-bold text-gray-900">{title}</DialogTitle>
    </div>
    <DialogDescription className="text-gray-500 text-sm">
      {description}
    </DialogDescription>
  </DialogHeader>
);

export const VideoInfoCard = ({ fileName }) => (
  <div className="bg-white border border-gray-200 rounded-lg p-3.5 flex items-center gap-3.5 shadow-sm">
    <div className="w-10 h-10 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center shrink-0 border border-blue-100">
      <VideoCamera size={20} weight="fill" />
    </div>
    <div className="flex-1 min-w-0">
      <p className="text-[11px] font-bold text-gray-400 uppercase tracking-wider mb-0.5">Selected Video</p>
      <p className="text-[14px] font-semibold text-gray-800 truncate" title={fileName}>
        {fileName}
      </p>
    </div>
  </div>
);

export const StatusBadge = ({ status }) => {
  const isActive = status === "Active";
  return (
    <Badge
      variant={isActive ? "success" : "secondary"}
      className={`px-2 py-0.5 flex items-center gap-1.5 w-fit font-medium text-xs border shadow-sm
        ${isActive ? 'bg-green-50 text-green-700 border-green-200 hover:bg-green-100' : 'bg-gray-100 text-gray-600 border-gray-200 hover:bg-gray-200'}`}
    >
      {isActive ? <CheckCircle size={14} weight="fill" /> : <XCircle size={14} weight="fill" />}
      {status}
    </Badge>
  );
};

export const ConfirmButton = ({ isLoading, onClick, children }) => (
  <Button
    onClick={onClick}
    disabled={isLoading}
    className="bg-[#2563EB] text-white hover:bg-blue-700 min-w-[130px] transition-all flex items-center justify-center gap-2 rounded-md shadow-sm h-10"
  >
    {isLoading ? (
      <>
        <span className="animate-spin h-4 w-4 border-2 border-white/30 border-t-white rounded-full"></span>
        Saving...
      </>
    ) : (
      <>
        <Check size={16} weight="bold" />
        {children}
      </>
    )}
  </Button>
);

export function EditVideoModal({ video, isOpen, onClose, onSave }) {
  const [status, setStatus] = useState("Active");
  const [isSaving, setIsSaving] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (video && isOpen) {
      setStatus(video.status);
      setErrorMsg("");
      setSuccess(false);
    }
  }, [video, isOpen]);

  // If no video but dialog is open, it might crash rendering, so return null early
  // unless we're closing and animating out.
  if (!video) return null;

  const handleSave = async () => {
    setIsSaving(true);
    setErrorMsg("");
    setSuccess(false);
    try {
      await onSave(video.id, status);
      setSuccess(true);
      // Wait a moment for the user to see the success state
      setTimeout(() => {
        onClose();
      }, 600);
    } catch (err) {
      setErrorMsg(err.message || "Failed to update video status.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && !isSaving && onClose()}>
      <DialogContent className="sm:max-w-md p-0 overflow-hidden border-0 shadow-2xl rounded-xl gap-0">
        <div className="px-6 py-5 bg-white">
          <ModalHeader
            title="Edit Video Status"
            description="Update whether this video is active in the presentation playlist."
            icon={PencilSimple}
          />
        </div>

        <div className="px-6 py-5 bg-[#F9FAFB] space-y-6 border-y border-gray-200 shadow-inner">
          <VideoInfoCard fileName={video.fileName} />

          <div className="space-y-3">
            <label className="text-sm font-semibold text-gray-700 block">Presentation Status</label>
            <div className="grid grid-cols-2 gap-3">
              <button
                onClick={() => setStatus("Active")}
                disabled={isSaving}
                className={`flex flex-col items-start gap-1 p-3.5 border-2 rounded-lg transition-all text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-1
                  ${status === "Active" 
                    ? "border-green-500 bg-green-50/50 shadow-sm ring-1 ring-green-500" 
                    : "border-gray-200 bg-white hover:border-green-300 hover:bg-green-50/30 text-gray-500"}`}
              >
                <div className="flex items-center justify-between w-full mb-1">
                  <span className={`font-bold text-[14px] ${status === "Active" ? "text-green-700" : "text-gray-600"}`}>Active</span>
                  <div className={`w-4 h-4 rounded-full border flex items-center justify-center transition-colors ${status === "Active" ? "border-green-600 bg-green-600" : "border-gray-300"}`}>
                    {status === "Active" && <div className="w-1.5 h-1.5 bg-white rounded-full" />}
                  </div>
                </div>
                <span className={`text-[12px] leading-tight ${status === "Active" ? "text-green-700/80" : "text-gray-400"}`}>Will play in presentation</span>
              </button>

              <button
                onClick={() => setStatus("Inactive")}
                disabled={isSaving}
                className={`flex flex-col items-start gap-1 p-3.5 border-2 rounded-lg transition-all text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-1
                  ${status === "Inactive" 
                    ? "border-gray-500 bg-gray-100 shadow-sm ring-1 ring-gray-500" 
                    : "border-gray-200 bg-white hover:border-gray-300 hover:bg-gray-50 text-gray-500"}`}
              >
                <div className="flex items-center justify-between w-full mb-1">
                  <span className={`font-bold text-[14px] ${status === "Inactive" ? "text-gray-800" : "text-gray-600"}`}>Inactive</span>
                  <div className={`w-4 h-4 rounded-full border flex items-center justify-center transition-colors ${status === "Inactive" ? "border-gray-600 bg-gray-600" : "border-gray-300"}`}>
                    {status === "Inactive" && <div className="w-1.5 h-1.5 bg-white rounded-full" />}
                  </div>
                </div>
                <span className={`text-[12px] leading-tight ${status === "Inactive" ? "text-gray-600" : "text-gray-400"}`}>Hidden from playlist</span>
              </button>
            </div>
          </div>

          {errorMsg && (
            <div className="p-3 rounded-md bg-red-50 border border-red-200 text-red-600 text-[13px] font-medium flex items-start gap-2">
              <XCircle size={18} weight="fill" className="shrink-0 mt-0.5" />
              <span>{errorMsg}</span>
            </div>
          )}
          
          {success && (
            <div className="p-3 rounded-md bg-green-50 border border-green-200 text-green-700 text-[13px] font-medium flex items-start gap-2">
              <CheckCircle size={18} weight="fill" className="shrink-0 mt-0.5" />
              <span>Successfully saved!</span>
            </div>
          )}
        </div>

        <DialogFooter className="px-6 py-4 bg-white flex-row sm:justify-between items-center w-full">
          <div className="flex flex-col sm:flex-row sm:items-center gap-2 max-sm:mb-3">
            <span className="text-[12px] font-medium text-gray-500 uppercase tracking-wider">Current:</span>
            <StatusBadge status={video.status} />
          </div>
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              onClick={onClose}
              disabled={isSaving}
              className="border-gray-200 text-gray-700 hover:bg-gray-100 h-10 rounded-md font-medium"
            >
              Cancel
            </Button>
            <ConfirmButton isLoading={isSaving} onClick={handleSave}>
              Save Changes
            </ConfirmButton>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
