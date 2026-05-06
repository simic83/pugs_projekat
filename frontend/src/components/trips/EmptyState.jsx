import { Inbox } from "lucide-react";

export function EmptyState({ children }) {
  return (
    <div className="empty-state">
      <Inbox className="empty-state-icon" aria-hidden="true" />
      <span>{children}</span>
    </div>
  );
}
