function navButton(name, label) {
  return `<button class="nav-item${state.view === name ? " active" : ""}" type="button" data-view="${name}"${state.view === name ? ' aria-current="page"' : ""}><span class="nav-icon">${icons[name]}</span>${label}</button>`;
}

function render() {
  if (!state.token || !state.user) {
    renderAuth();
    return;
  }
  renderShell();
  loadView();
}

function renderAuth() {
  const registering = state.authMode === "register";
  app.innerHTML = `
    <main class="auth-screen">
      <section class="auth-art">
        <div class="auth-brand"><img class="brand-mark" src="/image/school-seal.svg" alt="" /><div><div class="auth-brand-name">RTNCloud</div><div class="auth-brand-sub">School Management</div></div></div>
        <div class="auth-promo"><h1>One place for your school community.</h1><p>Keep student records, courses, and enrollments organized with a clear view of your school day.</p></div>
        <div class="auth-caption">A simple workspace for learning communities</div>
      </section>
      <section class="auth-card-wrap">
        <div class="auth-card">
          <h2>${registering ? "Create your account" : "Welcome back"}</h2>
          <p class="auth-intro">${registering ? "Register to access the school management portal." : "Sign in to continue to your school dashboard."}</p>
          <div class="notice auth-error" role="alert"></div>
          <form class="auth-form" id="auth-form">
            ${registering ? '<div class="field"><label for="auth-name">Full name</label><input id="auth-name" name="fullName" autocomplete="name" maxlength="120" required></div>' : ""}
            <div class="field"><label for="auth-email">Email address</label><input id="auth-email" name="email" type="email" autocomplete="email" maxlength="200" required></div>
            <div class="field"><label for="auth-password">Password</label><input id="auth-password" name="password" type="password" autocomplete="${registering ? "new-password" : "current-password"}" minlength="${registering ? "8" : "1"}" required></div>
            <button class="button primary" type="submit">${registering ? "Create account" : "Sign in"}</button>
          </form>
          <p class="auth-switch">${registering ? "Already have an account?" : "New to EduCloud?"} <button class="text-button" type="button" data-auth-mode="${registering ? "login" : "register"}">${registering ? "Sign in" : "Create an account"}</button></p>
        </div>
      </section>
    </main>`;
}

function renderShell() {
  const fullName = escapeHtml(state.user.fullName);
  app.innerHTML = `
    <div class="app-shell">
      <aside class="sidebar" id="sidebar">
        <div class="brand"><img class="brand-mark" src="/image/school-seal.svg" alt="" /><div class="brand-copy"><div class="brand-name">${escapeHtml(state.settings.schoolName || "RTNCloud")}</div><div class="brand-subtitle">School Management</div></div></div>
        <nav class="side-nav" aria-label="Main navigation">
          ${navButton("dashboard", "Dashboard")}
          ${navButton("students", "Students")}
            ${state.user.role === "Admin" ? navButton("teachers", "Teachers") : ""}
          ${navButton("courses", "Courses")}
          ${navButton("classes", "Classes")}
          ${navButton("enrollments", "Enrollments")}
          ${navButton("attendance", "Attendance")}
          ${navButton("grades", "Grades")}
          ${navButton("reports", "Reports")}
          ${navButton("settings", "Settings")}
        </nav>
        <div class="sidebar-spacer"></div>
        <div class="account">
          <span class="avatar">${escapeHtml(initials(state.user.fullName))}</span>
          <div class="account-copy"><div class="account-name">${fullName}</div><div class="account-email">${escapeHtml(state.user.email)} · ${escapeHtml(state.user.role)}</div></div>
          <button class="icon-button" type="button" data-action="logout" aria-label="Sign out" title="Sign out">${icons.logout}</button>
        </div>
      </aside>
      <div class="main-column">
        <header class="topbar">
          <button class="icon-button mobile-menu" type="button" data-action="menu" aria-label="Toggle navigation">${icons.grid}</button>
          <label class="search-box">${icons.search}<input id="global-search" type="search" placeholder="Search this page..." aria-label="Search current page" value="${escapeHtml(state.query)}"></label>
          <div class="topbar-spacer"></div>
          <div class="topbar-meta"><span>Signed in as <span class="meta-chip">${escapeHtml(state.user.role)}</span></span></div>
          <span class="avatar top-avatar" aria-hidden="true">${escapeHtml(initials(state.user.fullName))}</span>
        </header>
        <main class="main-content"><div id="page-content"></div></main>
      </div>
      <dialog class="modal" id="record-modal" aria-labelledby="modal-title"></dialog>
    </div>`;
}

function heading(title, subtitle, action = "") {
  return `<div class="page-heading"><div><h1>${title}</h1><p class="page-subtitle">${subtitle}</p></div>${action}</div>`;
}

function actionButton(label = "Add record") {
  if (state.user.role !== "Admin") return "";
  return `<button class="button primary" type="button" data-action="create"><span class="button-icon">${icons.plus}</span>${label}</button>`;
}

async function loadView() {
  const content = document.querySelector("#page-content");
  if (!content) return;
  content.innerHTML =
    '<div class="loading"><span class="spinner"></span>Loading school data...</div>';
  try {
    if (state.view === "dashboard") {
      state.settings = await request("/api/settings");
      state.attendanceDate = schoolDateValue();
      [state.students, state.courses, state.enrollments] = await Promise.all([
        request("/api/students"),
        request("/api/courses"),
        request("/api/enrollments"),
      ]);
      [state.classes, state.teachers, state.attendanceSummary] =
        await Promise.all([
          request("/api/classes"),
          state.user.role === "Admin"
            ? request("/api/teachers")
            : Promise.resolve([]),
          request(
            `/api/reports/attendance-summary?date=${state.attendanceDate}`,
          ),
        ]);
      content.innerHTML = renderDashboard();
      const brandName = document.querySelector(".brand-name");
      if (brandName) brandName.textContent = state.settings.schoolName;
    } else if (state.view === "students") {
      state.students = await request("/api/students");
      content.innerHTML = renderStudents();
    } else if (state.view === "courses") {
      state.courses = await request("/api/courses");
      content.innerHTML = renderCourses();
    } else if (state.view === "enrollments") {
      state.enrollments = await request("/api/enrollments");
      content.innerHTML = renderEnrollments();
    } else if (state.view === "teachers") {
      state.teachers = await request("/api/teachers");
      content.innerHTML = renderTeachers();
    } else if (state.view === "classes") {
      [state.classes, state.courses, state.teachers] = await Promise.all([
        request("/api/classes"),
        request("/api/courses"),
        state.user.role === "Admin"
          ? request("/api/teachers")
          : Promise.resolve([]),
      ]);
      if (state.rosterClassId) {
        state.roster = await request(
          `/api/classes/${state.rosterClassId}/roster`,
        );
        content.innerHTML = renderRoster();
      } else {
        content.innerHTML = renderClasses();
      }
    } else if (state.view === "attendance") {
      state.classes = await request("/api/classes");
      if (!state.selectedClassId && state.classes.length)
        state.selectedClassId = String(state.classes[0].id);
      state.attendance = state.selectedClassId
        ? await request(
            `/api/classes/${state.selectedClassId}/attendance?date=${state.attendanceDate}`,
          )
        : [];
      content.innerHTML = renderAttendance();
    } else if (state.view === "grades") {
      state.classes = await request("/api/classes");
      if (!state.selectedClassId && state.classes.length)
        state.selectedClassId = String(state.classes[0].id);
      state.assessments = state.selectedClassId
        ? await request(`/api/classes/${state.selectedClassId}/assessments`)
        : [];
      content.innerHTML = renderGrades();
    } else if (state.view === "reports") {
      state.classes = await request("/api/classes");
      content.innerHTML = renderReports();
    } else if (state.view === "settings") {
      state.settings = await request("/api/settings");
      content.innerHTML = renderSettings();
    }
    applySearch();
  } catch (error) {
    if (!state.token) return;
    content.innerHTML = `${heading("Unable to load data", "The API request could not be completed.")}<div class="notice" role="alert">${escapeHtml(errorMessage(error))}</div><button class="button" type="button" data-action="refresh"><span class="button-icon">${icons.refresh}</span>Try again</button>`;
  }
}

function renderDashboard() {
  const activeEnrollments = state.enrollments.filter(
    (item) => String(item.status).toLowerCase() === "active",
  ).length;
  const presentToday = state.attendanceSummary.presentCount;
  const absentToday = state.attendanceSummary.absentCount;
  const stats = [
    {
      label: "Total students",
      value: state.students.length,
      note: "Student records",
      icon: icons.people,
      tone: "",
    },
    {
      label: "Teachers",
      value: state.user.role === "Admin" ? state.teachers.length : "—",
      note:
        state.user.role === "Admin" ? "Teacher accounts" : "Admin view only",
      icon: icons.book,
      tone: "purple",
    },
    {
      label: "Classes",
      value: state.classes.length,
      note: "Active class sections",
      icon: icons.classes,
      tone: "amber",
    },
    {
      label: "Present today",
      value: presentToday,
      note: `${state.attendanceSummary.totalRecords} attendance records`,
      icon: icons.check,
      tone: "green",
    },
    {
      label: "Absent today",
      value: absentToday,
      note: "Marked absent",
      icon: icons.attendance,
      tone: "purple",
    },
    {
      label: "Active enrollments",
      value: activeEnrollments,
      note: "Current course registrations",
      icon: icons.check,
      tone: "",
    },
    {
      label: "Available courses",
      value: state.courses.length,
      note: "Course catalog",
      icon: icons.book,
      tone: "",
    },
  ];
  const courseCounts = state.courses
    .map((course) => ({
      name: course.name,
      count: state.enrollments.filter((item) => item.courseId === course.id)
        .length,
    }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 6);
  const maxCourseCount = Math.max(
    ...courseCounts.map((course) => course.count),
    1,
  );
  const bars = courseCounts.length
    ? courseCounts
        .map(
          (course) =>
            `<div class="course-bar-row"><span class="course-bar-name" title="${escapeHtml(course.name)}">${escapeHtml(course.name)}</span><div class="bar-track"><div class="bar-fill" style="width:${Math.max(course.count ? 4 : 0, (course.count / maxCourseCount) * 100)}%"></div></div><span class="course-bar-count">${course.count}</span></div>`,
        )
        .join("")
    : '<div class="table-empty">No courses are available yet.</div>';
  const recent = [...state.enrollments]
    .sort((a, b) => new Date(b.enrolledAtUtc) - new Date(a.enrolledAtUtc))
    .slice(0, 5);
  const activity = recent.length
    ? recent
        .map(
          (item) =>
            `<div class="activity-item"><span class="activity-avatar">${escapeHtml(initials(item.studentName))}</span><div class="activity-copy"><strong>${escapeHtml(item.studentName)}</strong> enrolled in <strong>${escapeHtml(item.courseName)}</strong><div class="activity-time">${escapeHtml(relativeDate(item.enrolledAtUtc))}</div></div></div>`,
        )
        .join("")
    : '<div class="table-empty">Enrollments will appear here when students are registered for courses.</div>';
  return `
    ${heading("Dashboard", "A live overview of students, courses, and enrollments.")}
    <section class="stats-grid" aria-label="School summary">
      ${stats.map((stat) => `<article class="stat-card"><div><div class="stat-label">${stat.label}</div><div class="stat-value">${stat.value}</div><div class="stat-note">${stat.note}</div></div><span class="stat-icon ${stat.tone}">${stat.icon}</span></article>`).join("")}
    </section>
    <section class="content-grid">
      <article class="panel"><div class="panel-heading"><div><h2 class="panel-title">Enrollments by course</h2><p class="panel-description">Current registrations across available courses</p></div><span class="stat-icon">${icons.courses}</span></div><div class="course-bars">${bars}</div></article>
      <article class="panel"><div class="panel-heading"><div><h2 class="panel-title">Recent enrollments</h2><p class="panel-description">Latest activity across the school</p></div><button class="button small" type="button" data-view="enrollments">View all</button></div><div class="activity-list">${activity}</div></article>
    </section>
    <p class="page-subtitle" style="margin-top:16px">Attendance counts are based on records entered for today. Grade summaries are available in the Grades and Reports sections.</p>`;
}
