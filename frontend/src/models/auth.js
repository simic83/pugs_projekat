export const USER_ROLES = {
  USER: "User",
  ADMIN: "Admin",
};

export const USER_ROLE_OPTIONS = [
  { value: USER_ROLES.USER, label: "User" },
  { value: USER_ROLES.ADMIN, label: "Admin" },
];

export function createRoleModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    name: normalizeRoleName(source.name ?? source),
  };
}

export function createUserModel(source = {}) {
  source = source ?? {};

  return {
    id: source.id ?? "",
    name: source.name ?? "",
    email: source.email ?? "",
    roles: Array.isArray(source.roles) ? source.roles.map(createRoleModel) : [],
    createdAtUtc: source.createdAtUtc ?? "",
  };
}

export function createAuthResponseModel(source = {}) {
  source = source ?? {};

  return {
    result: source.result ?? null,
    accessToken: source.accessToken ?? null,
    expiresAtUtc: source.expiresAtUtc ?? null,
    user: source.user ? createUserModel(source.user) : null,
  };
}

export function createLoginFormModel(source = {}) {
  source = source ?? {};

  return {
    email: source.email ?? "",
    password: source.password ?? "",
  };
}

export function createRegisterFormModel(source = {}) {
  source = source ?? {};

  return {
    name: source.name ?? "",
    email: source.email ?? "",
    password: source.password ?? "",
  };
}

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

export function normalizeUsers(users) {
  return Array.isArray(users) ? users.map(createUserModel) : [];
}
