export const USER_ROLES = {
  USER: "User",
  ADMIN: "Admin",
};

export function normalizeRoleName(role) {
  const value = typeof role === "object" && role !== null ? role.name : role;

  if (value === 1 || value === "1" || String(value).toLowerCase() === "admin") {
    return USER_ROLES.ADMIN;
  }

  if (value === 0 || value === "0" || String(value).toLowerCase() === "user") {
    return USER_ROLES.USER;
  }

  return "";
}

export function getRoleNames(roles = []) {
  return roles.map(normalizeRoleName).filter(Boolean);
}

export function userHasRole(user, roleName) {
  return getRoleNames(user?.roles).some((currentRole) => currentRole.toLowerCase() === roleName.toLowerCase());
}
