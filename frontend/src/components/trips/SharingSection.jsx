import { Ban, EyeOff, Link2, QrCode, Share2 } from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import { SHARE_ACCESS_LEVEL_OPTIONS } from "../../models/sharing.js";
import { EmptyState } from "./EmptyState.jsx";
import { FormFieldError } from "./FormFieldError.jsx";
import {
  buildSharedTripPlanLink,
  formatDateTime,
  getShareAccessLevelLabel,
  todayDateInput,
} from "./tripDisplayUtils.js";

export function SharingSection({
  errors = {},
  generatedShareLink,
  onAccessLevelChange,
  onExpiresAtChange,
  onRevoke,
  onSubmit,
  onToggleQr,
  shareAccessLevel,
  shareExpiresAt,
  shares,
  visibleShareQrId,
}) {
  return (
    <section className="section-card">
      <div className="section-header">
        <div>
          <h2 className="section-title section-title-row">
            <Share2 className="section-title-icon" aria-hidden="true" />
            Deljenje
          </h2>
          <p className="section-subtitle">Kreiranje i opoziv javnih linkova.</p>
        </div>
        <span className="badge">{shares.length}</span>
      </div>

      <div className="access-help">
        <strong>VIEW</strong>: samo pregled. <strong>EDIT</strong>: moze izmeniti osnovni plan i njegove stavke.
      </div>

      <form className="form-grid" noValidate onSubmit={onSubmit}>
        <div className="form-row">
          <label className="field">
            <span className="field-label">AccessLevel</span>
            <select
              className={`select${errors.accessLevel ? " input-error" : ""}`}
              name="accessLevel"
              onChange={(event) => onAccessLevelChange(Number(event.target.value))}
              value={shareAccessLevel}
            >
              {SHARE_ACCESS_LEVEL_OPTIONS.map((accessLevel) => (
                <option key={accessLevel.value} value={accessLevel.value}>
                  {accessLevel.label}
                </option>
              ))}
            </select>
            <FormFieldError message={errors.accessLevel} />
          </label>
          <label className="field">
            <span className="field-label">Datum isteka</span>
            <input
              className={`input${errors.expiresAt ? " input-error" : ""}`}
              min={todayDateInput()}
              name="expiresAt"
              onChange={(event) => onExpiresAtChange(event.target.value)}
              type="date"
              value={shareExpiresAt}
            />
            <FormFieldError message={errors.expiresAt} />
          </label>
        </div>

        <button className="btn btn-primary" type="submit">
          <Link2 className="btn-icon" aria-hidden="true" />
          Kreiraj link
        </button>
      </form>

      {generatedShareLink ? (
        <div className="generated-share">
          <p className="link-box">
            <Link2 className="link-icon" aria-hidden="true" />
            Novi link:{" "}
            <a href={generatedShareLink} rel="noreferrer" target="_blank">
              {generatedShareLink}
            </a>
          </p>
          <ShareQrCode value={generatedShareLink} />
        </div>
      ) : null}

      <div className="item-list">
        {shares.map((share) => {
          const shareLink = buildSharedTripPlanLink(share.token);
          const isQrVisible = visibleShareQrId === share.id;

          return (
            <article className="list-item" key={share.id}>
              <div className="list-item-main">
                <span className="list-item-title">{getShareAccessLevelLabel(share.accessLevel)}</span>
                <span className={`badge ${share.isRevoked ? "badge-danger" : "badge-success"}`}>
                  {share.isRevoked ? "Revoked" : "Active"}
                </span>
                <p className="muted">Created: {formatDateTime(share.createdAt)}</p>
                {share.expiresAt ? <p className="muted">Expires: {formatDateTime(share.expiresAt)}</p> : null}
                <p className="share-token">{share.token}</p>
                {!share.isRevoked ? (
                  <a className="breakable" href={shareLink} rel="noreferrer" target="_blank">
                    {shareLink}
                  </a>
                ) : null}
                {!share.isRevoked && isQrVisible ? <ShareQrCode value={shareLink} /> : null}
              </div>
              {!share.isRevoked ? (
                <div className="list-item-actions">
                  <button
                    className="btn btn-secondary btn-small"
                    onClick={() => onToggleQr(isQrVisible ? null : share.id)}
                    type="button"
                  >
                    {isQrVisible ? (
                      <EyeOff className="btn-icon" aria-hidden="true" />
                    ) : (
                      <QrCode className="btn-icon" aria-hidden="true" />
                    )}
                    {isQrVisible ? "Sakrij QR" : "Prikazi QR"}
                  </button>
                  <button className="btn btn-danger-soft btn-small" onClick={() => onRevoke(share.id)} type="button">
                    <Ban className="btn-icon" aria-hidden="true" />
                    Opozovi
                  </button>
                </div>
              ) : null}
            </article>
          );
        })}
        {shares.length === 0 ? <EmptyState>Nema kreiranih share tokena.</EmptyState> : null}
      </div>
    </section>
  );
}

function ShareQrCode({ value }) {
  return (
    <div className="share-qr-card">
      <div className="share-qr-code">
        <QRCodeSVG includeMargin level="M" size={132} title="QR kod za deljeni plan" value={value} />
      </div>
      <p>Skeniraj QR kod za otvaranje deljenog plana.</p>
    </div>
  );
}
