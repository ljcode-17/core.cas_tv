import { forwardRef } from "react";
import { Link } from "react-router-dom";

import { useSettings } from "@app/providers";
import { useMainLayout } from "@app/layouts/main/provider/LayoutProvider";

import { SidebarToggle } from "./SidebarToggle";

const SidebarHeader = forwardRef((props, ref) => {
  const { layout, sidebarMouseLeave } = useMainLayout();
  const { settings } = useSettings();

  const sidebarHeaderTitle = () => (
    <h1 className={`text-lg font-bold hidden md:flex ms-2.5 me-1 ${settings?.themeLayout === "horizontal" ? 'text-dark' : 'text-gray-900 dark:text-white'}`}>
      <span className="font-bold">ADMIN</span>
      <span className="font-normal ml-1">PORTAL</span>
    </h1>
  );

  return (
    <div
      ref={ref}
      className={`sidebar-header ${
        settings?.themeLayout === "horizontal"
          ? `bg-gray-200 `
          : `bg-transparent`
      } hidden lg:flex items-center relative justify-between lg:px-4 shrink-0 gap-2.5 p-6`}
    >
      <Link to="/" className="flex items-center justify-center ml-2">
        <img
          src="https://cdn.onecoredevit.com/logos/core-icon.svg"
          className="h-[34px]"
          alt="logo"
        />
        {layout.options.sidebar.collapse
          ? !sidebarMouseLeave && sidebarHeaderTitle()
          : sidebarHeaderTitle()}
      </Link>
      {settings.themeLayout === "horizontal" && <SidebarToggle />}
    </div>
  );
});
export { SidebarHeader };
