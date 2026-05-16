const fs = require("fs");
const path = require("path");

const ROOT = process.argv[2] || ".";
const BACKEND = path.join(ROOT, "backend");
const OUT_DIR = path.join(ROOT, "backend-learning-export");

const MAX_CHARS_PER_FILE = 350_000;

function norm(p) {
  return p.replace(/\\/g, "/");
}

function walk(dir) {
  let results = [];

  if (!fs.existsSync(dir)) return results;

  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    const normalized = norm(fullPath);

    if (
      normalized.includes("/bin/") ||
      normalized.includes("/obj/") ||
      normalized.includes("/packages/") ||
      normalized.includes("/TravelPlanner/pkg/")
    ) {
      continue;
    }

    if (entry.isDirectory()) {
      results = results.concat(walk(fullPath));
    } else {
      results.push(fullPath);
    }
  }

  return results;
}

function rel(file) {
  return norm(path.relative(ROOT, file));
}

function isCsprojOrSln(p) {
  return p.startsWith("backend/") && (p.endsWith(".csproj") || p.endsWith("TravelPlanner.sln"));
}

function shouldIncludeContracts(p) {
  return (
    p.startsWith("backend/Contracts/Interfaces/") && p.endsWith(".cs") ||
    p.startsWith("backend/Contracts/Enums/") && p.endsWith(".cs") ||
    p.startsWith("backend/Contracts/Common/") && p.endsWith(".cs") ||
    p.startsWith("backend/Contracts/") && p.endsWith("Dto.cs")
  );
}

function shouldIncludeApiGateway(p) {
  return (
    p === "backend/ApiGatewayService/ApiGatewayService.cs" ||
    p.startsWith("backend/ApiGatewayService/Controllers/") && p.endsWith(".cs") ||
    p.startsWith("backend/ApiGatewayService/Infrastructure/") && p.endsWith(".cs") ||
    p.startsWith("backend/ApiGatewayService/Configuration/") && p.endsWith(".cs")
  );
}

function shouldIncludeIdentity(p) {
  return (
    p === "backend/IdentityService/IdentityService.cs" ||
    p.startsWith("backend/IdentityService/Data/") && p.endsWith(".cs") ||
    p.startsWith("backend/IdentityService/Security/") && p.endsWith(".cs") ||
    p.startsWith("backend/IdentityService/Validation/") && p.endsWith(".cs") ||
    p.startsWith("backend/IdentityService/Mapping/") && p.endsWith(".cs") ||
    p.startsWith("backend/IdentityService/Models/") && p.endsWith(".cs") ||
    p.startsWith("backend/IdentityService/Configuration/") && p.endsWith(".cs")
  );
}

function shouldIncludeTripPlanning(p) {
  return (
    p === "backend/TripPlanningService/TripPlanningService.cs" ||
    p.startsWith("backend/TripPlanningService/Data/") && p.endsWith(".cs") ||
    p.startsWith("backend/TripPlanningService/Models/") && p.endsWith(".cs") ||
    p.startsWith("backend/TripPlanningService/Configuration/") && p.endsWith(".cs")
  );
}

function shouldIncludeBudget(p) {
  return (
    p === "backend/BudgetService/BudgetService.cs" ||
    p.startsWith("backend/BudgetService/Data/") && p.endsWith(".cs") ||
    p.startsWith("backend/BudgetService/Models/") && p.endsWith(".cs") ||
    p.startsWith("backend/BudgetService/Configuration/") && p.endsWith(".cs")
  );
}

function shouldIncludeSharing(p) {
  return (
    p === "backend/SharingService/SharingService.cs" ||
    p.startsWith("backend/SharingService/Data/") && p.endsWith(".cs") ||
    p.startsWith("backend/SharingService/Models/") && p.endsWith(".cs") ||
    p.startsWith("backend/SharingService/Configuration/") && p.endsWith(".cs")
  );
}

function shouldIncludeDatabase(p) {
  return (
    p.startsWith("backend/database/") &&
    (p.endsWith(".sql") || p.endsWith(".md"))
  );
}

function shouldIncludeConfigOverview(p) {
  return (
    p.endsWith("/PackageRoot/Config/Settings.xml") ||
    p === "backend/TravelPlanner/ApplicationPackageRoot/ApplicationManifest.xml" ||
    p.startsWith("backend/TravelPlanner/ApplicationParameters/") && p.endsWith(".xml") ||
    p === "backend/TravelPlanner/StartupServices.xml" ||
    p.endsWith("/PackageRoot/ServiceManifest.xml") ||
    isCsprojOrSln(p)
  );
}

function readFileSafe(file) {
  try {
    return fs.readFileSync(file, "utf8");
  } catch {
    return "[GREŠKA: Fajl nije moguće pročitati kao tekst]";
  }
}

function writeGroupedOutput(groupName, files) {
  if (files.length === 0) {
    console.log(`Preskačem ${groupName} - nema fajlova.`);
    return;
  }

  let part = 1;
  let buffer = "";

  function flush() {
    if (!buffer.trim()) return;

    const fileName = `${groupName}${part > 1 ? `_part${part}` : ""}.txt`;
    const outPath = path.join(OUT_DIR, fileName);

    fs.writeFileSync(outPath, buffer, "utf8");
    console.log(`Kreiran: ${norm(outPath)}`);

    part++;
    buffer = "";
  }

  for (const file of files) {
    const relativePath = rel(file);
    const content = readFileSafe(file);

    const block =
`================================================================================
FILE: ${relativePath}
================================================================================

${content}

\n\n`;

    if (buffer.length + block.length > MAX_CHARS_PER_FILE) {
      flush();
    }

    buffer += block;
  }

  flush();
}

function main() {
  if (!fs.existsSync(BACKEND)) {
    console.error("Nije pronađen backend folder. Pokreni skriptu iz root foldera projekta.");
    process.exit(1);
  }

  fs.mkdirSync(OUT_DIR, { recursive: true });

  const allFiles = walk(BACKEND).sort((a, b) => rel(a).localeCompare(rel(b)));

  const groups = [
    {
      name: "01_Contracts",
      filter: shouldIncludeContracts
    },
    {
      name: "02_ApiGatewayService",
      filter: shouldIncludeApiGateway
    },
    {
      name: "03_IdentityService",
      filter: shouldIncludeIdentity
    },
    {
      name: "04_TripPlanningService",
      filter: shouldIncludeTripPlanning
    },
    {
      name: "05_BudgetService",
      filter: shouldIncludeBudget
    },
    {
      name: "06_SharingService",
      filter: shouldIncludeSharing
    },
    {
      name: "07_Database",
      filter: shouldIncludeDatabase
    },
    {
      name: "08_Config_Overview",
      filter: shouldIncludeConfigOverview
    }
  ];

  for (const group of groups) {
    const files = allFiles.filter(file => group.filter(rel(file)));
    writeGroupedOutput(group.name, files);
  }

  console.log("\nGotovo.");
  console.log(`TXT fajlovi su u folderu: ${norm(OUT_DIR)}`);
}

main();