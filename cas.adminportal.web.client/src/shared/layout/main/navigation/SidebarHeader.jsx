import { forwardRef, useState } from "react";
import { Link } from "react-router-dom";

import { useSettings } from "@app/providers";
import { useMainLayout } from "@app/layouts/main/provider/LayoutProvider";
import { toAbsoluteUrl } from "@shared/utils/assets";

import { SidebarToggle } from "./SidebarToggle";

const SidebarHeader = forwardRef((props, ref) => {
  const { layout, sidebarMouseLeave } = useMainLayout();

  const { settings } = useSettings();

  const brandLogo = () => {
    return (
      <Link to="/" className="flex items-center gap-3 ml-2">
        <div className="flex items-center justify-center w-8 h-8 rounded-full bg-[#E60012] shrink-0">
          <div className="w-[18px] h-[18px] rounded-full border-2 border-white"></div>
        </div>
        {(!layout.options.sidebar.collapse || sidebarMouseLeave) && (
          <h1 className="text-[#111827] text-[14px] font-bold leading-tight flex flex-col uppercase tracking-wide hidden md:flex">
            <span>CORE® AGILE</span>
            <span>SYSTEM</span>
          </h1>
        )}
      </Link>
    );
  };

  return (
    <div
      ref={ref}
      className={`sidebar-header ${settings?.themeLayout === "horizontal"
          ? `bg-gray-200 `
          : `bg-transparent`
        } hidden lg:flex items-center relative justify-between lg:px-4 shrink-0 gap-2.5 p-6`}
    >
      {brandLogo()}
      {settings.themeLayout === "horizontal" && <SidebarToggle />}
    </div>
  );
});
export { SidebarHeader };
