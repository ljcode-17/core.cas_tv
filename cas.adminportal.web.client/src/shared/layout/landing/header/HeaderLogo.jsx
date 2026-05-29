import { Link } from "react-router-dom";
import { toAbsoluteUrl } from "@shared/utils";

const HeaderLogo = () => {
  return (
    <div className="flex gap-1 items-center -ms-1 font-bold">
      <Link to="/" className="flex items-center gap-3 ml-2">
        <div className="flex items-center justify-center shrink-0">
          <img
            src={toAbsoluteUrl('/core-logo.png')}
            className="w-[32px] h-[32px] object-contain"
            alt="Logo"
          />
        </div>
        <h1 className="text-[#111827] text-[14px] leading-tight flex uppercase tracking-wide hidden md:flex">
          <span className="font-bold mr-1">ADMIN</span>
          <span className="font-normal">PORTAL</span>
        </h1>
      </Link>
    </div>
  );
};
export { HeaderLogo };
