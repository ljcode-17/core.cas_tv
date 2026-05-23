import { forwardRef } from "react";
import { Bell, Minus, X } from "@phosphor-icons/react";

const HeaderTopbar = forwardRef((props, ref) => {
  return (
    <div
      ref={ref}
      className="flex items-center justify-end shrink-0 gap-6"
    >
      <div className="flex items-center justify-center gap-5 text-[#6B7280]">
        <Bell size={20} className="cursor-pointer hover:text-black transition-colors" />
        <Minus size={20} className="cursor-pointer hover:text-black transition-colors" />
        <X size={20} className="cursor-pointer hover:text-black transition-colors" />
      </div>
    </div>
  );
});
export { HeaderTopbar };
