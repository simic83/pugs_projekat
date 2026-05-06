import { ActivityCalendar } from "../ActivityCalendar.jsx";

export function ActivityCalendarView({ activities, initialDate }) {
  return <ActivityCalendar activities={activities} emptyMessage="Nema dodatih aktivnosti." initialDate={initialDate} />;
}
