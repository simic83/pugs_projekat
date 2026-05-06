const activityStatuses = [
  { value: 0, label: "Planned" },
  { value: 1, label: "Reserved" },
  { value: 2, label: "Completed" },
  { value: 3, label: "Cancelled" },
];

export function ActivityCalendar({ days, emptyMessage = "Nema aktivnosti za prikaz." }) {
  if (days.length === 0) {
    return <div className="empty-state">{emptyMessage}</div>;
  }

  return (
    <div className="activity-calendar">
      {days.map((day) => (
        <section
          className={`activity-calendar-day${day.activities.length === 0 ? " is-empty" : ""}`}
          key={day.dateKey}
        >
          <header className="activity-calendar-day-header">
            <span className="activity-calendar-date">
              {day.dateKey === "no-date" ? "Bez datuma" : formatDateKey(day.dateKey)}
            </span>
            <span className="badge badge-muted">{day.activities.length}</span>
          </header>

          {day.activities.length > 0 ? (
            <div className="activity-calendar-items">
              {day.activities.map((activity) => (
                <ActivityCalendarItem activity={activity} key={activity.id} />
              ))}
            </div>
          ) : (
            <p className="activity-calendar-empty">Nema aktivnosti.</p>
          )}
        </section>
      ))}
    </div>
  );
}

export function buildActivityCalendarDays(startDate, endDate, activities) {
  const sortedActivities = sortActivitiesByDateAndTime(activities);
  const activitiesByDate = sortedActivities.reduce((groups, activity) => {
    const dateKey = toDateInputValue(activity.activityDate);
    if (!dateKey) {
      return groups;
    }

    groups[dateKey] = groups[dateKey] ?? [];
    groups[dateKey].push(activity);
    return groups;
  }, {});

  const rangeDateKeys = buildDateRangeKeys(toDateInputValue(startDate), toDateInputValue(endDate));
  const activityDateKeys = Object.keys(activitiesByDate).sort(compareDates);
  const dateKeys =
    rangeDateKeys.length > 0
      ? Array.from(new Set([...rangeDateKeys, ...activityDateKeys])).sort(compareDates)
      : activityDateKeys;
  const noDateActivities = sortedActivities.filter((activity) => !toDateInputValue(activity.activityDate));

  const days = dateKeys.map((dateKey) => ({
    dateKey,
    activities: activitiesByDate[dateKey] ?? [],
  }));

  if (noDateActivities.length > 0) {
    days.push({
      dateKey: "no-date",
      activities: noDateActivities,
    });
  }

  return days;
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

function ActivityCalendarItem({ activity }) {
  const details = [
    activity.activityTime ? String(activity.activityTime).slice(0, 5) : "",
    activity.location ?? "",
    hasEstimatedCost(activity.estimatedCost) ? formatMoney(activity.estimatedCost) : "",
  ].filter(Boolean);

  return (
    <article className="activity-calendar-item">
      <div className="activity-calendar-item-header">
        <span className="activity-calendar-item-title">{activity.title}</span>
        <span className={`badge ${getStatusClass(activity.status)}`}>{getStatusLabel(activity.status)}</span>
      </div>

      {details.length > 0 ? (
        <p className="activity-calendar-meta">
          {details.map((detail, index) => (
            <span key={`${detail}-${index}`}>{detail}</span>
          ))}
        </p>
      ) : null}

      {activity.description ? <p className="list-item-description">{activity.description}</p> : null}
    </article>
  );
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

function buildDateRangeKeys(startKey, endKey) {
  const startDate = parseDateKey(startKey);
  const endDate = parseDateKey(endKey);

  if (!startDate || !endDate || startDate.getTime() > endDate.getTime()) {
    return [];
  }

  const keys = [];
  const currentDate = new Date(startDate.getTime());

  while (currentDate.getTime() <= endDate.getTime()) {
    keys.push(toDateKey(currentDate));
    currentDate.setDate(currentDate.getDate() + 1);
  }

  return keys;
}

function parseDateKey(dateKey) {
  if (!dateKey) {
    return null;
  }

  const [year, month, day] = String(dateKey).split("-").map(Number);
  const date = new Date(year, month - 1, day);

  if (!year || !month || !day || Number.isNaN(date.getTime())) {
    return null;
  }

  return date;
}

function toDateKey(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 10);
}

function compareDates(firstValue, secondValue) {
  const firstTime = firstValue ? new Date(firstValue).getTime() : Number.MAX_SAFE_INTEGER;
  const secondTime = secondValue ? new Date(secondValue).getTime() : Number.MAX_SAFE_INTEGER;

  return firstTime - secondTime;
}

function formatDateKey(dateKey) {
  const date = parseDateKey(dateKey);
  return date ? date.toLocaleDateString() : "";
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function hasEstimatedCost(value) {
  return value !== null && value !== undefined && value !== "" && Number(value) > 0;
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
