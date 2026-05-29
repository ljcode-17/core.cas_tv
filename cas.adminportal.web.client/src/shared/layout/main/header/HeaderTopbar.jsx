import { forwardRef } from "react";
import { NotificationContent } from "../components/NotificationContent";
import { AccountContent } from "../components/AccountContent";

const HeaderTopbar = forwardRef((props, ref) => {
  return (
    <div
      ref={ref}
      className="flex items-center justify-end shrink-0 gap-3"
    >
      <NotificationContent />
      <AccountContent />
    </div>
  );
});

HeaderTopbar.displayName = "HeaderTopbar";
export { HeaderTopbar };
