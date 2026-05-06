import { useMemo } from "react";
import FullCalendar from "@fullcalendar/react";
import dayGridPlugin from "@fullcalendar/daygrid";

const calendarPlugins = [dayGridPlugin];

const activityStatuses = [
  { value: 0, label: "Planned" },
  { value: 1, label: "Reserved" },
  { value: 2, label: "Completed" },
  { value: 3, label: "Cancelled" },
];

export function ActivityCalendar({ activities = [], emptyMessage = "Nema aktivnosti za prikaz.", initialDate }) {
  const sortedActivities = useMemo(() => sortActivitiesByDateAndTime(activities), [activities]);
  const calendarEvents = useMemo(() => buildCalendarEvents(sortedActivities), [sortedActivities]);
  const activitiesWithoutDate = useMemo(
    () => sortedActivities.filter((activity) => !toDateInputValue(activity.activityDate)),
    [sortedActivities],
  );
  const initialCalendarDate = useMemo(
    () => getInitialCalendarDate(initialDate, sortedActivities),
    [initialDate, sortedActivities],
  );

  return (
    <div className="activity-calendar">
      <FullCalendar
        buttonIcons={false}
        buttonText={{
          next: "Sledeci",
          prev: "Prethodni",
        }}
        dayMaxEvents={3}
        displayEventTime={false}
        eventDisplay="block"
        events={calendarEvents}
        firstDay={1}
        fixedWeekCount={false}
        headerToolbar={{
          center: "title",
          left: "prev",
          right: "next",
        }}
        height="auto"
        initialDate={initialCalendarDate}
        initialView="dayGridMonth"
        key={initialCalendarDate}
        plugins={calendarPlugins}
      />

      {activitiesWithoutDate.length > 0 ? (
        <section className="activity-calendar-no-date">
          <div className="activity-calendar-no-date-header">
            <span className="activity-calendar-no-date-title">Bez datuma</span>
            <span className="badge badge-muted">{activitiesWithoutDate.length}</span>
          </div>

          <div className="activity-calendar-no-date-list">
            {activitiesWithoutDate.map((activity) => (
              <article className="activity-calendar-no-date-item" key={activity.id ?? activity.title}>
                <span>{buildEventTitle(activity)}</span>
                <span className={`badge ${getStatusClass(activity.status)}`}>{getStatusLabel(activity.status)}</span>
              </article>
            ))}
          </div>
        </section>
      ) : null}

      {activities.length === 0 ? <div className="empty-state activity-calendar-message">{emptyMessage}</div> : null}
    </div>
  );
}

export function groupActivitiesByDate(activities) {
  return Object.entries(
    sortActivitiesByDateAndTime(activities).reduce((groups, activity) => {
      const key = activity.activityDate ? toDateInputValue(activity.activityDate) : "no-date";
      groups[key] = groups[key] ?? [];
      groups[key].push(activity);
      return groups;
    }, {}),
  );
}

function buildCalendarEvents(activities) {
  return activities
    .filter((activity) => toDateInputValue(activity.activityDate))
    .map((activity) => ({
      allDay: true,
      backgroundColor: getEventColor(activity.status),
      borderColor: getEventColor(activity.status),
      classNames: ["activity-calendar-event", getStatusClass(activity.status)],
      date: toDateInputValue(activity.activityDate),
      extendedProps: {
        location: activity.location ?? "",
        status: getStatusLabel(activity.status),
      },
      id: String(activity.id ?? `${activity.activityDate}-${activity.title}`),
      textColor: "#ffffff",
      title: buildEventTitle(activity),
    }));
}

function buildEventTitle(activity) {
  const time = activity.activityTime ? String(activity.activityTime).slice(0, 5) : "";
  const title = activity.title || "Aktivnost";
  const status = getStatusLabel(activity.status);

  return `${time ? `${time} - ` : ""}${title} (${status})`;
}

function getInitialCalendarDate(initialDate, activities) {
  const initialDateKey = toDateInputValue(initialDate);
  if (isValidDateKey(initialDateKey)) {
    return initialDateKey;
  }

  const firstActivityDateKey = activities.map((activity) => toDateInputValue(activity.activityDate)).find(isValidDateKey);
  if (firstActivityDateKey) {
    return firstActivityDateKey;
  }

  return toDateKey(new Date());
}

function sortActivitiesByDateAndTime(activities) {
  return [...activities].sort((first, second) => {
    const dateResult = compareDates(first.activityDate, second.activityDate);
    if (dateResult !== 0) {
      return dateResult;
    }

    return String(first.activityTime ?? "").localeCompare(String(second.activityTime ?? ""));
  });
}

function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 10);
}

function toDateKey(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function isValidDateKey(dateKey) {
  if (!dateKey) {
    return false;
  }

  const [year, month, day] = String(dateKey).split("-").map(Number);
  const date = new Date(year, month - 1, day);

  return (
    Boolean(year && month && day) &&
    !Number.isNaN(date.getTime()) &&
    date.getFullYear() === year &&
    date.getMonth() === month - 1 &&
    date.getDate() === day
  );
}

function compareDates(firstValue, secondValue) {
  const firstDateKey = toDateInputValue(firstValue);
  const secondDateKey = toDateInputValue(secondValue);
  const firstTime = isValidDateKey(firstDateKey) ? new Date(firstDateKey).getTime() : Number.MAX_SAFE_INTEGER;
  const secondTime = isValidDateKey(secondDateKey) ? new Date(secondDateKey).getTime() : Number.MAX_SAFE_INTEGER;

  return firstTime - secondTime;
}

function getStatusLabel(value) {
  const numericValue = Number(value);
  return activityStatuses.find((status) => status.value === numericValue)?.label ?? "Planned";
}

function getStatusClass(value) {
  const numericValue = Number(value);

  if (numericValue === 1) {
    return "status-reserved";
  }

  if (numericValue === 2) {
    return "status-completed";
  }

  if (numericValue === 3) {
    return "status-cancelled";
  }

  return "status-planned";
}

function getEventColor(value) {
  const numericValue = Number(value);

  if (numericValue === 1) {
    return "#c2410c";
  }

  if (numericValue === 2) {
    return "#0f766e";
  }

  if (numericValue === 3) {
    return "#b42318";
  }

  return "#0f4c81";
}
