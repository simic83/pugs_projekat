import { useCallback, useEffect, useState } from "react";
import { activitiesApi } from "../api/activitiesApi.js";
import { budgetApi } from "../api/budgetApi.js";
import { checklistApi } from "../api/checklistApi.js";
import { destinationsApi } from "../api/destinationsApi.js";
import { sharingApi } from "../api/sharingApi.js";
import { tripPlansApi } from "../api/tripPlansApi.js";
import { useAuth } from "../context/AuthContext.jsx";
import { EXPENSE_CATEGORIES } from "../models/budget.js";
import { SHARE_ACCESS_LEVEL_OPTIONS, SHARE_ACCESS_LEVELS } from "../models/sharing.js";

const emptyTripPlanForm = {
  title: "",
  description: "",
  startDate: "",
  endDate: "",
  plannedBudget: "0",
  notes: "",
};

const emptyDestinationForm = {
  name: "",
  location: "",
  arrivalDate: "",
  departureDate: "",
  description: "",
};

const emptyActivityForm = {
  title: "",
  activityDate: "",
  activityTime: "",
  location: "",
  description: "",
  estimatedCost: "0",
  status: 0,
};

const emptyExpenseForm = {
  title: "",
  category: 0,
  amount: "0",
  expenseDate: "",
  description: "",
};

const emptyChecklistForm = {
  title: "",
};

const activityStatuses = [
  { value: 0, label: "Planned" },
  { value: 1, label: "Reserved" },
  { value: 2, label: "Completed" },
  { value: 3, label: "Cancelled" },
];

export function TripPlansPage() {
  const { logout, user } = useAuth();
  const [tripPlans, setTripPlans] = useState([]);
  const [selectedTripPlanId, setSelectedTripPlanId] = useState(null);
  const [selectedTripPlan, setSelectedTripPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [checklistItems, setChecklistItems] = useState([]);
  const [shares, setShares] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [tripPlanForm, setTripPlanForm] = useState(emptyTripPlanForm);
  const [destinationForm, setDestinationForm] = useState(emptyDestinationForm);
  const [activityForm, setActivityForm] = useState(emptyActivityForm);
  const [expenseForm, setExpenseForm] = useState(emptyExpenseForm);
  const [checklistForm, setChecklistForm] = useState(emptyChecklistForm);
  const [shareAccessLevel, setShareAccessLevel] = useState(SHARE_ACCESS_LEVELS.VIEW);
  const [shareExpiresAt, setShareExpiresAt] = useState("");
  const [generatedShareLink, setGeneratedShareLink] = useState("");
  const [editingExpenseId, setEditingExpenseId] = useState(null);
  const [editingChecklistItemId, setEditingChecklistItemId] = useState(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const loadTripPlans = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const plans = await tripPlansApi.getAll();
      setTripPlans(plans ?? []);
      setSelectedTripPlanId((currentId) => currentId ?? plans?.[0]?.id ?? null);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadTripPlanDetails = useCallback(async (tripPlanId) => {
    if (!tripPlanId) {
      setSelectedTripPlan(null);
      setDestinations([]);
      setActivities([]);
      setExpenses([]);
      setChecklistItems([]);
      setShares([]);
      setBudgetSummary(null);
      setEditingExpenseId(null);
      setEditingChecklistItemId(null);
      setExpenseForm(emptyExpenseForm);
      setChecklistForm(emptyChecklistForm);
      setGeneratedShareLink("");
      return;
    }

    setError("");

    try {
      const [tripPlan, tripDestinations, tripActivities, tripExpenses, tripBudgetSummary, tripChecklistItems, tripShares] =
        await Promise.all([
          tripPlansApi.getById(tripPlanId),
          destinationsApi.getByTripPlanId(tripPlanId),
          activitiesApi.getByTripPlanId(tripPlanId),
          budgetApi.getExpenses(tripPlanId),
          budgetApi.getBudgetSummary(tripPlanId),
          checklistApi.getChecklistItems(tripPlanId),
          sharingApi.getShares(tripPlanId),
        ]);

      setSelectedTripPlan(tripPlan);
      setDestinations(tripDestinations ?? []);
      setActivities(tripActivities ?? []);
      setExpenses(tripExpenses ?? []);
      setBudgetSummary(tripBudgetSummary);
      setChecklistItems(tripChecklistItems ?? []);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  }, []);

  useEffect(() => {
    void loadTripPlans();
  }, [loadTripPlans]);

  useEffect(() => {
    void loadTripPlanDetails(selectedTripPlanId);
  }, [loadTripPlanDetails, selectedTripPlanId]);

  const updateTripPlanField = (event) => {
    updateForm(setTripPlanForm, event);
  };

  const updateDestinationField = (event) => {
    updateForm(setDestinationForm, event);
  };

  const updateActivityField = (event) => {
    updateForm(setActivityForm, event);
  };

  const updateExpenseField = (event) => {
    updateForm(setExpenseForm, event);
  };

  const updateChecklistField = (event) => {
    updateForm(setChecklistForm, event);
  };

  const submitTripPlan = async (event) => {
    event.preventDefault();
    setError("");

    try {
      const created = await tripPlansApi.create({
        title: tripPlanForm.title,
        description: tripPlanForm.description || null,
        startDate: toDateTime(tripPlanForm.startDate),
        endDate: toDateTime(tripPlanForm.endDate),
        plannedBudget: Number(tripPlanForm.plannedBudget || 0),
        notes: tripPlanForm.notes || null,
      });

      setTripPlanForm(emptyTripPlanForm);
      await loadTripPlans();
      setSelectedTripPlanId(created.id);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteTripPlan = async (tripPlanId) => {
    setError("");

    try {
      await tripPlansApi.remove(tripPlanId);
      setSelectedTripPlanId(null);
      setSelectedTripPlan(null);
      setExpenses([]);
      setChecklistItems([]);
      setShares([]);
      setBudgetSummary(null);
      setEditingChecklistItemId(null);
      setChecklistForm(emptyChecklistForm);
      setGeneratedShareLink("");
      await loadTripPlans();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitDestination = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await destinationsApi.create(selectedTripPlanId, {
        name: destinationForm.name,
        location: destinationForm.location || null,
        arrivalDate: toDateTime(destinationForm.arrivalDate),
        departureDate: toDateTime(destinationForm.departureDate),
        description: destinationForm.description || null,
      });

      setDestinationForm(emptyDestinationForm);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteDestination = async (destinationId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await destinationsApi.remove(selectedTripPlanId, destinationId);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitActivity = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await activitiesApi.create(selectedTripPlanId, {
        title: activityForm.title,
        activityDate: toDateTime(activityForm.activityDate),
        activityTime: activityForm.activityTime ? `${activityForm.activityTime}:00` : null,
        location: activityForm.location || null,
        description: activityForm.description || null,
        estimatedCost: Number(activityForm.estimatedCost || 0),
        status: Number(activityForm.status),
      });

      setActivityForm(emptyActivityForm);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteActivity = async (activityId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await activitiesApi.remove(selectedTripPlanId, activityId);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const createShare = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const created = await sharingApi.createShare(selectedTripPlanId, {
        accessLevel: Number(shareAccessLevel),
        expiresAt: shareExpiresAt ? toExpirationDateTime(shareExpiresAt) : null,
      });

      setShareExpiresAt("");
      setGeneratedShareLink(buildSharedTripPlanLink(created.token));
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const revokeShare = async (shareId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await sharingApi.revokeShare(selectedTripPlanId, shareId);
      setGeneratedShareLink("");
      const tripShares = await sharingApi.getShares(selectedTripPlanId);
      setShares(tripShares ?? []);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitExpense = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: expenseForm.title,
        category: Number(expenseForm.category),
        amount: Number(expenseForm.amount || 0),
        expenseDate: toDateTime(expenseForm.expenseDate),
        description: expenseForm.description || null,
      };

      if (editingExpenseId) {
        await budgetApi.updateExpense(selectedTripPlanId, editingExpenseId, payload);
      } else {
        await budgetApi.createExpense(selectedTripPlanId, payload);
      }

      setExpenseForm(emptyExpenseForm);
      setEditingExpenseId(null);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editExpense = (expense) => {
    setEditingExpenseId(expense.id);
    setExpenseForm({
      title: expense.title ?? "",
      category: Number(expense.category ?? 0),
      amount: String(expense.amount ?? 0),
      expenseDate: toDateInputValue(expense.expenseDate),
      description: expense.description ?? "",
    });
  };

  const cancelExpenseEdit = () => {
    setEditingExpenseId(null);
    setExpenseForm(emptyExpenseForm);
  };

  const deleteExpense = async (expenseId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await budgetApi.deleteExpense(selectedTripPlanId, expenseId);
      if (editingExpenseId === expenseId) {
        cancelExpenseEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const submitChecklistItem = async (event) => {
    event.preventDefault();
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      const payload = {
        tripPlanId: selectedTripPlanId,
        title: checklistForm.title,
      };

      if (editingChecklistItemId) {
        const checklistItem = checklistItems.find((item) => item.id === editingChecklistItemId);
        await checklistApi.updateChecklistItem(selectedTripPlanId, editingChecklistItemId, {
          ...payload,
          isCompleted: Boolean(checklistItem?.isCompleted),
        });
      } else {
        await checklistApi.createChecklistItem(selectedTripPlanId, payload);
      }

      setChecklistForm(emptyChecklistForm);
      setEditingChecklistItemId(null);
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editChecklistItem = (checklistItem) => {
    setEditingChecklistItemId(checklistItem.id);
    setChecklistForm({
      title: checklistItem.title ?? "",
    });
  };

  const cancelChecklistEdit = () => {
    setEditingChecklistItemId(null);
    setChecklistForm(emptyChecklistForm);
  };

  const toggleChecklistItem = async (checklistItem, isCompleted) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await checklistApi.updateChecklistItem(selectedTripPlanId, checklistItem.id, {
        title: checklistItem.title,
        isCompleted,
      });

      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const deleteChecklistItem = async (checklistItemId) => {
    if (!selectedTripPlanId) {
      return;
    }

    setError("");

    try {
      await checklistApi.deleteChecklistItem(selectedTripPlanId, checklistItemId);
      if (editingChecklistItemId === checklistItemId) {
        cancelChecklistEdit();
      }
      await loadTripPlanDetails(selectedTripPlanId);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <main style={styles.page}>
      <header style={styles.header}>
        <div>
          <h1 style={styles.title}>Trip plans</h1>
          <p style={styles.subtitle}>{user?.name ?? user?.email ?? "User"}</p>
        </div>
        <button onClick={logout} style={styles.secondaryButton} type="button">
          Logout
        </button>
      </header>

      {error ? <p style={styles.error}>{error}</p> : null}

      <section style={styles.layout}>
        <aside style={styles.panel}>
          <form onSubmit={submitTripPlan} style={styles.form}>
            <h2 style={styles.sectionTitle}>New plan</h2>
            <input
              name="title"
              onChange={updateTripPlanField}
              placeholder="Title"
              required
              style={styles.input}
              value={tripPlanForm.title}
            />
            <textarea
              name="description"
              onChange={updateTripPlanField}
              placeholder="Description"
              rows={3}
              style={styles.input}
              value={tripPlanForm.description}
            />
            <div style={styles.twoColumns}>
              <label style={styles.field}>
                <span>Start</span>
                <input
                  name="startDate"
                  onChange={updateTripPlanField}
                  required
                  style={styles.input}
                  type="date"
                  value={tripPlanForm.startDate}
                />
              </label>
              <label style={styles.field}>
                <span>End</span>
                <input
                  name="endDate"
                  onChange={updateTripPlanField}
                  required
                  style={styles.input}
                  type="date"
                  value={tripPlanForm.endDate}
                />
              </label>
            </div>
            <input
              min="0"
              name="plannedBudget"
              onChange={updateTripPlanField}
              placeholder="Planned budget"
              step="0.01"
              style={styles.input}
              type="number"
              value={tripPlanForm.plannedBudget}
            />
            <textarea
              name="notes"
              onChange={updateTripPlanField}
              placeholder="Notes"
              rows={3}
              style={styles.input}
              value={tripPlanForm.notes}
            />
            <button style={styles.primaryButton} type="submit">
              Add plan
            </button>
          </form>

          <div style={styles.list}>
            <h2 style={styles.sectionTitle}>Plans</h2>
            {isLoading ? <p style={styles.muted}>Loading...</p> : null}
            {tripPlans.map((tripPlan) => (
              <button
                key={tripPlan.id}
                onClick={() => setSelectedTripPlanId(tripPlan.id)}
                style={{
                  ...styles.listItem,
                  ...(tripPlan.id === selectedTripPlanId ? styles.selectedListItem : null),
                }}
                type="button"
              >
                <strong>{tripPlan.title}</strong>
                <span>{formatDateRange(tripPlan.startDate, tripPlan.endDate)}</span>
              </button>
            ))}
            {!isLoading && tripPlans.length === 0 ? <p style={styles.muted}>No trip plans.</p> : null}
          </div>
        </aside>

        <section style={styles.panel}>
          {selectedTripPlan ? (
            <>
              <div style={styles.detailHeader}>
                <div>
                  <h2 style={styles.detailTitle}>{selectedTripPlan.title}</h2>
                  <p style={styles.muted}>{formatDateRange(selectedTripPlan.startDate, selectedTripPlan.endDate)}</p>
                </div>
                <button
                  onClick={() => deleteTripPlan(selectedTripPlan.id)}
                  style={styles.dangerButton}
                  type="button"
                >
                  Delete
                </button>
              </div>

              {selectedTripPlan.description ? <p>{selectedTripPlan.description}</p> : null}
              {selectedTripPlan.notes ? <p style={styles.note}>{selectedTripPlan.notes}</p> : null}

              <div style={styles.summaryGrid}>
                <p style={styles.summaryItem}>
                  <span>Planned budget</span>
                  <strong>{formatMoney(budgetSummary?.plannedBudget ?? selectedTripPlan.plannedBudget)}</strong>
                </p>
                <p style={styles.summaryItem}>
                  <span>Total expenses</span>
                  <strong>{formatMoney(budgetSummary?.totalExpenses)}</strong>
                </p>
                <p style={styles.summaryItem}>
                  <span>Remaining budget</span>
                  <strong>{formatMoney(budgetSummary?.remainingBudget ?? selectedTripPlan.plannedBudget)}</strong>
                </p>
              </div>

              <section style={styles.sharingSection}>
                <form onSubmit={createShare} style={styles.inlineForm}>
                  <h3 style={styles.sectionTitle}>Sharing</h3>
                  <select
                    onChange={(event) => setShareAccessLevel(Number(event.target.value))}
                    style={styles.input}
                    value={shareAccessLevel}
                  >
                    {SHARE_ACCESS_LEVEL_OPTIONS.map((accessLevel) => (
                      <option key={accessLevel.value} value={accessLevel.value}>
                        {accessLevel.label}
                      </option>
                    ))}
                  </select>
                  <input
                    aria-label="Datum isteka"
                    min={new Date().toISOString().slice(0, 10)}
                    onChange={(event) => setShareExpiresAt(event.target.value)}
                    style={styles.input}
                    type="date"
                    value={shareExpiresAt}
                  />
                  <button style={styles.primaryButton} type="submit">
                    Kreiraj link
                  </button>
                </form>

                {generatedShareLink ? (
                  <p style={styles.generatedLink}>
                    Novi link:{" "}
                    <a href={generatedShareLink} rel="noreferrer" style={styles.link} target="_blank">
                      {generatedShareLink}
                    </a>
                  </p>
                ) : null}

                <div style={styles.list}>
                  {shares.map((share) => {
                    const shareLink = buildSharedTripPlanLink(share.token);

                    return (
                      <div key={share.id} style={styles.rowItem}>
                        <div>
                          <strong>{getShareAccessLevelLabel(share.accessLevel)}</strong>
                          <p style={styles.shareToken}>{share.token}</p>
                          <p style={styles.muted}>
                            {share.isRevoked ? "Revoked" : "Active"} - created {formatDateTime(share.createdAt)}
                          </p>
                          {share.expiresAt ? (
                            <p style={styles.muted}>Expires {formatDateTime(share.expiresAt)}</p>
                          ) : null}
                          {!share.isRevoked ? (
                            <a href={shareLink} rel="noreferrer" style={styles.link} target="_blank">
                              {shareLink}
                            </a>
                          ) : null}
                        </div>
                        {!share.isRevoked ? (
                          <button
                            onClick={() => revokeShare(share.id)}
                            style={styles.smallDangerButton}
                            type="button"
                          >
                            Opozovi
                          </button>
                        ) : null}
                      </div>
                    );
                  })}
                  {shares.length === 0 ? <p style={styles.muted}>No share tokens.</p> : null}
                </div>
              </section>

              <div style={styles.detailGrid}>
                <section style={styles.subsection}>
                  <form onSubmit={submitDestination} style={styles.form}>
                    <h3 style={styles.sectionTitle}>Destinations</h3>
                    <input
                      name="name"
                      onChange={updateDestinationField}
                      placeholder="Name"
                      required
                      style={styles.input}
                      value={destinationForm.name}
                    />
                    <input
                      name="location"
                      onChange={updateDestinationField}
                      placeholder="Location"
                      style={styles.input}
                      value={destinationForm.location}
                    />
                    <div style={styles.twoColumns}>
                      <label style={styles.field}>
                        <span>Arrival</span>
                        <input
                          name="arrivalDate"
                          onChange={updateDestinationField}
                          required
                          style={styles.input}
                          type="date"
                          value={destinationForm.arrivalDate}
                        />
                      </label>
                      <label style={styles.field}>
                        <span>Departure</span>
                        <input
                          name="departureDate"
                          onChange={updateDestinationField}
                          required
                          style={styles.input}
                          type="date"
                          value={destinationForm.departureDate}
                        />
                      </label>
                    </div>
                    <button style={styles.primaryButton} type="submit">
                      Add destination
                    </button>
                  </form>

                  <div style={styles.list}>
                    {destinations.map((destination) => (
                      <div key={destination.id} style={styles.rowItem}>
                        <div>
                          <strong>{destination.name}</strong>
                          <p style={styles.muted}>
                            {destination.location ? `${destination.location} - ` : ""}
                            {formatDateRange(destination.arrivalDate, destination.departureDate)}
                          </p>
                        </div>
                        <button
                          onClick={() => deleteDestination(destination.id)}
                          style={styles.smallDangerButton}
                          type="button"
                        >
                          Delete
                        </button>
                      </div>
                    ))}
                    {destinations.length === 0 ? <p style={styles.muted}>No destinations.</p> : null}
                  </div>
                </section>

                <section style={styles.subsection}>
                  <form onSubmit={submitActivity} style={styles.form}>
                    <h3 style={styles.sectionTitle}>Activities</h3>
                    <input
                      name="title"
                      onChange={updateActivityField}
                      placeholder="Title"
                      required
                      style={styles.input}
                      value={activityForm.title}
                    />
                    <div style={styles.twoColumns}>
                      <input
                        name="activityDate"
                        onChange={updateActivityField}
                        required
                        style={styles.input}
                        type="date"
                        value={activityForm.activityDate}
                      />
                      <input
                        name="activityTime"
                        onChange={updateActivityField}
                        style={styles.input}
                        type="time"
                        value={activityForm.activityTime}
                      />
                    </div>
                    <input
                      name="location"
                      onChange={updateActivityField}
                      placeholder="Location"
                      style={styles.input}
                      value={activityForm.location}
                    />
                    <div style={styles.twoColumns}>
                      <input
                        min="0"
                        name="estimatedCost"
                        onChange={updateActivityField}
                        placeholder="Estimated cost"
                        step="0.01"
                        style={styles.input}
                        type="number"
                        value={activityForm.estimatedCost}
                      />
                      <select
                        name="status"
                        onChange={updateActivityField}
                        style={styles.input}
                        value={activityForm.status}
                      >
                        {activityStatuses.map((status) => (
                          <option key={status.value} value={status.value}>
                            {status.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <button style={styles.primaryButton} type="submit">
                      Add activity
                    </button>
                  </form>

                  <div style={styles.list}>
                    {activities.map((activity) => (
                      <div key={activity.id} style={styles.rowItem}>
                        <div>
                          <strong>{activity.title}</strong>
                          <p style={styles.muted}>
                            {formatDate(activity.activityDate)}
                            {activity.activityTime ? ` ${String(activity.activityTime).slice(0, 5)}` : ""}
                            {activity.location ? ` - ${activity.location}` : ""}
                          </p>
                          <p style={styles.muted}>
                            {getStatusLabel(activity.status)} - {formatMoney(activity.estimatedCost)}
                          </p>
                        </div>
                        <button
                          onClick={() => deleteActivity(activity.id)}
                          style={styles.smallDangerButton}
                          type="button"
                        >
                          Delete
                        </button>
                      </div>
                    ))}
                    {activities.length === 0 ? <p style={styles.muted}>No activities.</p> : null}
                  </div>
                </section>

                <section style={styles.subsection}>
                  <form onSubmit={submitExpense} style={styles.form}>
                    <h3 style={styles.sectionTitle}>{editingExpenseId ? "Edit expense" : "Expenses"}</h3>
                    <input
                      name="title"
                      onChange={updateExpenseField}
                      placeholder="Title"
                      required
                      style={styles.input}
                      value={expenseForm.title}
                    />
                    <div style={styles.twoColumns}>
                      <select
                        name="category"
                        onChange={updateExpenseField}
                        required
                        style={styles.input}
                        value={expenseForm.category}
                      >
                        {EXPENSE_CATEGORIES.map((category) => (
                          <option key={category.value} value={category.value}>
                            {category.label}
                          </option>
                        ))}
                      </select>
                      <input
                        min="0"
                        name="amount"
                        onChange={updateExpenseField}
                        placeholder="Amount"
                        required
                        step="0.01"
                        style={styles.input}
                        type="number"
                        value={expenseForm.amount}
                      />
                    </div>
                    <input
                      name="expenseDate"
                      onChange={updateExpenseField}
                      required
                      style={styles.input}
                      type="date"
                      value={expenseForm.expenseDate}
                    />
                    <textarea
                      name="description"
                      onChange={updateExpenseField}
                      placeholder="Description"
                      rows={3}
                      style={styles.input}
                      value={expenseForm.description}
                    />
                    <div style={styles.buttonRow}>
                      <button style={styles.primaryButton} type="submit">
                        {editingExpenseId ? "Save expense" : "Add expense"}
                      </button>
                      {editingExpenseId ? (
                        <button onClick={cancelExpenseEdit} style={styles.secondaryButton} type="button">
                          Cancel
                        </button>
                      ) : null}
                    </div>
                  </form>

                  <div style={styles.list}>
                    {expenses.map((expense) => (
                      <div key={expense.id} style={styles.rowItem}>
                        <div>
                          <strong>{expense.title}</strong>
                          <p style={styles.muted}>
                            {getExpenseCategoryLabel(expense.category)} - {formatDate(expense.expenseDate)}
                          </p>
                          <p style={styles.muted}>{formatMoney(expense.amount)}</p>
                          {expense.description ? <p style={styles.muted}>{expense.description}</p> : null}
                        </div>
                        <div style={styles.rowActions}>
                          <button
                            onClick={() => editExpense(expense)}
                            style={styles.smallSecondaryButton}
                            type="button"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => deleteExpense(expense.id)}
                            style={styles.smallDangerButton}
                            type="button"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    ))}
                    {expenses.length === 0 ? <p style={styles.muted}>No expenses.</p> : null}
                  </div>
                </section>

                <section style={styles.subsection}>
                  <form onSubmit={submitChecklistItem} style={styles.form}>
                    <h3 style={styles.sectionTitle}>Checklist</h3>
                    <input
                      maxLength={150}
                      name="title"
                      onChange={updateChecklistField}
                      placeholder="Passport, ticket, charger..."
                      required
                      style={styles.input}
                      value={checklistForm.title}
                    />
                    <div style={styles.buttonRow}>
                      <button style={styles.primaryButton} type="submit">
                        {editingChecklistItemId ? "Save item" : "Add item"}
                      </button>
                      {editingChecklistItemId ? (
                        <button onClick={cancelChecklistEdit} style={styles.secondaryButton} type="button">
                          Cancel
                        </button>
                      ) : null}
                    </div>
                  </form>

                  <div style={styles.list}>
                    {checklistItems.map((checklistItem) => (
                      <div key={checklistItem.id} style={styles.checklistRow}>
                        <label style={styles.checklistLabel}>
                          <input
                            checked={Boolean(checklistItem.isCompleted)}
                            onChange={(event) => toggleChecklistItem(checklistItem, event.target.checked)}
                            type="checkbox"
                          />
                          <span style={checklistItem.isCompleted ? styles.completedText : null}>
                            {checklistItem.title}
                          </span>
                        </label>
                        <div style={styles.rowActions}>
                          <button
                            onClick={() => editChecklistItem(checklistItem)}
                            style={styles.smallSecondaryButton}
                            type="button"
                          >
                            Izmeni
                          </button>
                          <button
                            onClick={() => deleteChecklistItem(checklistItem.id)}
                            style={styles.smallDangerButton}
                            type="button"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    ))}
                    {checklistItems.length === 0 ? <p style={styles.muted}>No checklist items.</p> : null}
                  </div>
                </section>
              </div>
            </>
          ) : (
            <p style={styles.muted}>Select or create a trip plan.</p>
          )}
        </section>
      </section>
    </main>
  );
}

function updateForm(setForm, event) {
  const { name, value } = event.target;
  setForm((currentForm) => ({
    ...currentForm,
    [name]: value,
  }));
}

function toDateTime(dateValue) {
  return `${dateValue}T00:00:00`;
}

function toExpirationDateTime(dateValue) {
  return `${dateValue}T23:59:59`;
}

function formatDate(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleDateString();
}

function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString();
}

function toDateInputValue(value) {
  if (!value) {
    return "";
  }

  return String(value).slice(0, 10);
}

function formatDateRange(startDate, endDate) {
  return `${formatDate(startDate)} - ${formatDate(endDate)}`;
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function getStatusLabel(value) {
  const numericValue = Number(value);
  return activityStatuses.find((status) => status.value === numericValue)?.label ?? "Planned";
}

function getExpenseCategoryLabel(value) {
  const numericValue = Number(value);
  return EXPENSE_CATEGORIES.find((category) => category.value === numericValue)?.label ?? "Other";
}

function getShareAccessLevelLabel(value) {
  return Number(value) === SHARE_ACCESS_LEVELS.EDIT ? "EDIT" : "VIEW";
}

function buildSharedTripPlanLink(token) {
  return `${window.location.origin}/shared/${token}`;
}

const styles = {
  page: {
    minHeight: "100vh",
    padding: "24px",
    background: "#f7f7f8",
    color: "#202027",
  },
  header: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "16px",
    marginBottom: "18px",
  },
  title: {
    margin: 0,
    fontSize: "30px",
  },
  subtitle: {
    margin: "4px 0 0",
    color: "#60606b",
  },
  layout: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 320px), 1fr))",
    gap: "18px",
  },
  panel: {
    display: "grid",
    alignContent: "start",
    gap: "18px",
    padding: "18px",
    border: "1px solid #dedee3",
    borderRadius: "8px",
    background: "#ffffff",
  },
  form: {
    display: "grid",
    gap: "10px",
  },
  sectionTitle: {
    margin: 0,
    fontSize: "18px",
  },
  input: {
    width: "100%",
    minHeight: "38px",
    boxSizing: "border-box",
    padding: "8px 10px",
    border: "1px solid #c9c9d1",
    borderRadius: "6px",
    font: "inherit",
  },
  field: {
    display: "grid",
    gap: "4px",
    fontSize: "13px",
    color: "#565661",
  },
  twoColumns: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 170px), 1fr))",
    gap: "10px",
  },
  primaryButton: {
    minHeight: "40px",
    padding: "0 14px",
    border: 0,
    borderRadius: "6px",
    background: "#1769aa",
    color: "#ffffff",
    cursor: "pointer",
    font: "inherit",
    fontWeight: 700,
  },
  secondaryButton: {
    minHeight: "38px",
    padding: "0 14px",
    border: "1px solid #b9bbc6",
    borderRadius: "6px",
    background: "#ffffff",
    cursor: "pointer",
    font: "inherit",
  },
  buttonRow: {
    display: "flex",
    flexWrap: "wrap",
    gap: "10px",
  },
  dangerButton: {
    minHeight: "38px",
    padding: "0 14px",
    border: 0,
    borderRadius: "6px",
    background: "#b42318",
    color: "#ffffff",
    cursor: "pointer",
    font: "inherit",
  },
  smallDangerButton: {
    minHeight: "32px",
    padding: "0 10px",
    border: "1px solid #f0b8b2",
    borderRadius: "6px",
    background: "#fff5f4",
    color: "#b42318",
    cursor: "pointer",
    font: "inherit",
  },
  smallSecondaryButton: {
    minHeight: "32px",
    padding: "0 10px",
    border: "1px solid #b9bbc6",
    borderRadius: "6px",
    background: "#ffffff",
    color: "#202027",
    cursor: "pointer",
    font: "inherit",
  },
  list: {
    display: "grid",
    gap: "10px",
  },
  listItem: {
    display: "grid",
    gap: "4px",
    width: "100%",
    padding: "12px",
    border: "1px solid #dedee3",
    borderRadius: "6px",
    background: "#ffffff",
    cursor: "pointer",
    font: "inherit",
    textAlign: "left",
  },
  selectedListItem: {
    borderColor: "#1769aa",
    background: "#eef6fd",
  },
  detailHeader: {
    display: "flex",
    alignItems: "start",
    justifyContent: "space-between",
    gap: "16px",
  },
  detailTitle: {
    margin: 0,
    fontSize: "24px",
  },
  detailGrid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 280px), 1fr))",
    gap: "18px",
    marginTop: "10px",
  },
  subsection: {
    display: "grid",
    alignContent: "start",
    gap: "14px",
  },
  rowItem: {
    display: "flex",
    alignItems: "start",
    justifyContent: "space-between",
    gap: "12px",
    padding: "12px",
    border: "1px solid #dedee3",
    borderRadius: "6px",
  },
  rowActions: {
    display: "flex",
    flexWrap: "wrap",
    gap: "8px",
    justifyContent: "flex-end",
  },
  checklistRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    padding: "12px",
    border: "1px solid #dedee3",
    borderRadius: "6px",
  },
  checklistLabel: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
  },
  completedText: {
    color: "#60606b",
    textDecoration: "line-through",
  },
  muted: {
    margin: 0,
    color: "#60606b",
    fontSize: "14px",
  },
  note: {
    padding: "10px",
    borderRadius: "6px",
    background: "#f3f4f6",
  },
  summaryGrid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 160px), 1fr))",
    gap: "10px",
  },
  summaryItem: {
    display: "grid",
    gap: "4px",
    margin: 0,
    padding: "10px",
    border: "1px solid #dedee3",
    borderRadius: "6px",
    color: "#60606b",
  },
  sharingSection: {
    display: "grid",
    gap: "12px",
    paddingTop: "4px",
  },
  inlineForm: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 150px), 1fr))",
    gap: "10px",
    alignItems: "center",
  },
  generatedLink: {
    margin: 0,
    padding: "10px",
    border: "1px solid #b7d7ee",
    borderRadius: "6px",
    background: "#eef6fd",
    overflowWrap: "anywhere",
  },
  shareToken: {
    margin: "4px 0",
    color: "#60606b",
    fontSize: "13px",
    overflowWrap: "anywhere",
  },
  link: {
    color: "#1769aa",
    overflowWrap: "anywhere",
  },
  error: {
    margin: "0 0 16px",
    padding: "10px 12px",
    border: "1px solid #f0b8b2",
    borderRadius: "6px",
    background: "#fff5f4",
    color: "#b42318",
  },
};
