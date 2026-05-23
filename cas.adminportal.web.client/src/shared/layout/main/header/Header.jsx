import clsx from "clsx";
import { useMainLayout } from "@app/layouts/main/provider/LayoutProvider";

import { Breadcrumbs } from "@shared/layout/main/breadcrumbs/Breadcrumbs";

// Components
import { Container } from "@shared/components/container";

// Items
import { HeaderLogo } from "./HeaderLogo";
import { HeaderTopbar } from "./HeaderTopBar";

import { useOffSetTop } from "@shared/hooks";

import { HEADER } from "@shared/config/theme.config";

const Header = () => {
  const isOffset = useOffSetTop(HEADER.H_DASHBOARD_DESKTOP);

  const { headerSticky } = useMainLayout();

  // calc(${HEADER.H_DASHBOARD_DESKTOP - 16}) ${isOffset && `backdrop-blur-lg bg-white/30`}

  return (
    <header
      className={clsx(
        `header fixed top-0 z-10 lg:left-[250px] right-0 h-[60px] flex items-stretch shrink-0 bg-white border-b border-[#E5E7EB]`,
        headerSticky && "shadow-sm"
      )}
    >
      <Container className="flex justify-between items-center w-full h-full px-[28px]">
        <div className="flex items-center gap-3">
          <HeaderLogo />
          <div className="hidden lg:flex items-center gap-3">
             <div className="flex items-center justify-center w-[26px] h-[26px] rounded-full bg-[#E60012] shrink-0">
               <div className="w-[14px] h-[14px] rounded-full border-2 border-white"></div>
             </div>
             <h1 className="text-[#111827] text-[14px] font-semibold uppercase tracking-wide">
               CORE® AGILE SYSTEM
             </h1>
          </div>
        </div>
        <HeaderTopbar />
      </Container>
    </header>
  );
};

export { Header };
