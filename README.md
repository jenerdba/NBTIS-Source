# ✅ NBTIS-Source Rules of Engagement (Strict Mode)

---

## 🔒 1. Engagement & Governance

- **1.1** Do not assume anything.  
- **1.2** If you are unsure, ask me for clarification.  
- **1.3** You must comply with all the rules below before showing me any code or answers.  
- **1.4** You must not invent, rename, omit, or modify default template files or structure unless explicitly told.  
- **1.5** All generated code must target **.NET 10** and **C# 14** with `<LangVersion>preview</LangVersion>`.

---

## 🧱 2. Architecture

- **2.1** Strict **Vertical Slice Architecture** across all layers.  
- **2.2** Blazor Web App must use **InteractiveServer render mode** with **Global interactivity location**.  
- **2.3** All projects (`NBTIS.Web`, `ApiService`, `AppHost`, `ServiceDefaults`) must conform to the official **Visual Studio 2022 v17.14 Preview** project templates for .NET 10. No invented structure.  
- **2.4** Use `MapGroup()`, `TypedResults`, and route filters for Minimal API v2. **FastEndpoints is not allowed.**

---

## 💡 3. Code Quality & Style

- **3.1** All code must compile **cleanly** with **zero warnings or errors**. Assume `TreatWarningsAsErrors=true`.  
- **3.2** All code must include **full XML documentation**: `<summary>`, `<param>`, `<returns>`, etc.  
- **3.3** No unnecessary classes, abstractions, or allocations. Use `readonly`, `sealed`, and optimized logic.
- **3.4** One class per file.  
- **3.5** No hardcoded values for configuration. Use `appsettings.json` and `builder.Configuration[]`.

---

## 📦 4. Usings & Dependencies

- **4.1** Only include `using` statements that are **explicitly used** in the file.  
- **4.2** All `using` directives must be **sorted and minimal**.  
- **4.3** Do not include references for other Blazor hosting models (e.g., WebAssembly or Hybrid).  
- **4.4** Syncfusion Blazor components are allowed **only when Bootstrap 5 cannot meet the functional requirement** (e.g., data grids).

---

## 🎨 5. UI & Styling

- **5.1** Use **plain Bootstrap 5.3.6** and **Bootstrap Icons 1.11.3**, both via **LibMan**.  
- **5.2** All layout, menus, and pages must use native Bootstrap components unless Syncfusion is justified.  
- **5.3** All layout and styling must support:
  - Accessible focus outlines
  - Keyboard navigation
  - Proper semantic roles
  - `Skip to content` support (`#main-content`)
  - Adequate target sizes

---

## 🛡️ 6. Security & Compliance

- **6.1** Never include passwords, keys, or secrets in code. Use configuration.  
- **6.2** Do not use deprecated or obsolete APIs. Respect `[Obsolete]` attributes and version-specific docs.  
- **6.3** Use only public APIs from .NET 10 or officially documented NuGet packages.  
- **6.4** All UI and logic must comply with:
  - **WCAG 2.2**
  - **Section 508**
  - **WAI-ARIA**

---

## ⚙️ 7. Performance

- **7.1** Use **EF Core compiled queries** and in-memory caching where applicable.  
- **7.2** Use **response compression**, **rate limiting**, and **custom health checks** for production readiness.  
- **7.3** Avoid allocations and overuse of LINQ in hot paths.
