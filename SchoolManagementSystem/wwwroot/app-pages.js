function renderStudents() {
  const rows = state.students
    .map(
      (student) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(student.firstName)} ${escapeHtml(student.lastName)}</div><div class="secondary-cell">${escapeHtml(student.studentCode)}</div></td>
    <td>${escapeHtml(student.email)}</td><td>${escapeHtml(student.gender)}</td><td>${escapeHtml(formatDate(student.dateOfBirth))}</td><td>${escapeHtml(student.phone || "—")}</td>
    ${state.user.role === "Admin" ? `<td><div class="table-actions"><button class="icon-button" type="button" data-action="edit-student" data-id="${student.id}" aria-label="Edit ${escapeHtml(student.firstName)}" title="Edit">${icons.edit}</button><button class="icon-button" type="button" data-action="delete-student" data-id="${student.id}" aria-label="Delete ${escapeHtml(student.firstName)}" title="Delete">${icons.trash}</button></div></td>` : ""}
  </tr>`,
    )
    .join("");
  return `${heading("Students", "View and manage student records.", actionButton("Add student"))}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Student directory</h2><p class="panel-description">${state.students.length} student record${state.students.length === 1 ? "" : "s"}</p></div></div></div>
      <div class="table-wrap"><table><thead><tr><th>Student</th><th>Email</th><th>Gender</th><th>Date of birth</th><th>Phone</th>${state.user.role === "Admin" ? '<th><span class="sr-only">Actions</span></th>' : ""}</tr></thead><tbody>${rows || `<tr><td class="table-empty" colspan="${state.user.role === "Admin" ? 6 : 5}">No students found. ${state.user.role === "Admin" ? "Add a student to get started." : ""}</td></tr>`}</tbody></table></div>
    </section>`;
}

function renderCourses() {
  const rows = state.courses
    .map(
      (course) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(course.name)}</div><div class="secondary-cell">${escapeHtml(course.courseCode)}</div></td>
    <td>${escapeHtml(course.description || "—")}</td><td>${escapeHtml(course.credits)}</td>
    ${state.user.role === "Admin" ? `<td><div class="table-actions"><button class="icon-button" type="button" data-action="edit-course" data-id="${course.id}" aria-label="Edit ${escapeHtml(course.name)}" title="Edit">${icons.edit}</button><button class="icon-button" type="button" data-action="delete-course" data-id="${course.id}" aria-label="Delete ${escapeHtml(course.name)}" title="Delete">${icons.trash}</button></div></td>` : ""}
  </tr>`,
    )
    .join("");
  return `${heading("Courses", "Browse courses and their details.", actionButton("Add course"))}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Course catalog</h2><p class="panel-description">${state.courses.length} course${state.courses.length === 1 ? "" : "s"} available</p></div></div></div>
      <div class="table-wrap"><table><thead><tr><th>Course</th><th>Description</th><th>Credits</th>${state.user.role === "Admin" ? '<th><span class="sr-only">Actions</span></th>' : ""}</tr></thead><tbody>${rows || `<tr><td class="table-empty" colspan="${state.user.role === "Admin" ? 4 : 3}">No courses found.</td></tr>`}</tbody></table></div>
    </section>`;
}

function renderEnrollments() {
  const rows = [...state.enrollments]
    .sort((a, b) => new Date(b.enrolledAtUtc) - new Date(a.enrolledAtUtc))
    .map(
      (item) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(item.studentName)}</div><div class="secondary-cell">Student #${escapeHtml(item.studentId)}</div></td>
    <td>${escapeHtml(item.courseName)}<div class="secondary-cell">${item.classSectionCode ? `Class ${escapeHtml(item.classSectionCode)}` : `Course #${escapeHtml(item.courseId)} · No class assigned`}</div></td>
    <td><span class="badge">${escapeHtml(item.status)}</span></td><td>${escapeHtml(formatDate(item.enrolledAtUtc))}</td>
    ${state.user.role === "Admin" ? `<td><div class="table-actions"><button class="icon-button" type="button" data-action="delete-enrollment" data-id="${item.id}" aria-label="Remove enrollment for ${escapeHtml(item.studentName)} in ${escapeHtml(item.courseName)}" title="Remove enrollment">${icons.trash}</button></div></td>` : ""}
  </tr>`,
    )
    .join("");
  return `${heading("Enrollments", "Track students registered for each class.", actionButton("Add enrollment"))}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Class enrollments</h2><p class="panel-description">${state.enrollments.length} enrollment${state.enrollments.length === 1 ? "" : "s"}</p></div></div></div>
      <div class="table-wrap"><table><thead><tr><th>Student</th><th>Course</th><th>Status</th><th>Enrolled</th>${state.user.role === "Admin" ? '<th><span class="sr-only">Actions</span></th>' : ""}</tr></thead><tbody>${rows || `<tr><td class="table-empty" colspan="${state.user.role === "Admin" ? 5 : 4}">No enrollments yet.${state.user.role === "Admin" ? " Add an enrollment to get started." : ""}</td></tr>`}</tbody></table></div>
    </section>`;
}

function selectField(name, label, options, selected, full = false) {
  const choices = options
    .map(
      (option) =>
        `<option value="${escapeHtml(option.value)}"${String(option.value) === String(selected) ? " selected" : ""}>${escapeHtml(option.label)}</option>`,
    )
    .join("");
  return `<div class="field${full ? " full" : ""}"><label for="field-${name}">${label}</label><select id="field-${name}" name="${name}">${choices}</select></div>`;
}

function classOptions(selected = "") {
  return state.classes
    .map((item) => ({
      value: item.id,
      label: `${item.sectionCode} · ${item.courseName} · ${item.term}`,
    }))
    .map(
      (option) =>
        `<option value="${escapeHtml(option.value)}"${String(option.value) === String(selected) ? " selected" : ""}>${escapeHtml(option.label)}</option>`,
    )
    .join("");
}

function renderTeachers() {
  const rows = state.teachers
    .map(
      (teacher) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(teacher.fullName)}</div><div class="secondary-cell">Teacher</div></td>
    <td>${escapeHtml(teacher.email)}</td><td>${escapeHtml(formatDate(teacher.createdAtUtc))}</td>
  </tr>`,
    )
    .join("");
  return `${heading("Teachers", "Manage school staff accounts.", actionButton("Add teacher"))}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Teacher directory</h2><p class="panel-description">${state.teachers.length} teacher account${state.teachers.length === 1 ? "" : "s"}</p></div></div></div>
    <div class="table-wrap"><table><thead><tr><th>Teacher</th><th>Email</th><th>Account created</th></tr></thead><tbody>${rows || '<tr><td class="table-empty" colspan="3">No teacher accounts yet. Add a teacher to create a staff login.</td></tr>'}</tbody></table></div></section>`;
}

function renderClasses() {
  const rows = state.classes
    .map(
      (item) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(item.courseName)}</div><div class="secondary-cell">${escapeHtml(item.courseCode)}</div></td>
    <td><span class="badge">${escapeHtml(item.sectionCode)}</span></td><td>${escapeHtml(item.term)}</td>
    <td>${escapeHtml(item.teacherName)}</td><td>${escapeHtml(item.schedule || "—")}</td>
    <td>${escapeHtml(item.room || "—")}</td><td>${item.studentCount}</td>
    <td><div class="table-actions"><button class="button small" type="button" data-action="view-roster" data-id="${item.id}">Roster</button>${state.user.role === "Admin" ? `<button class="icon-button" type="button" data-action="edit-class" data-id="${item.id}" aria-label="Edit class ${escapeHtml(item.sectionCode)}">${icons.edit}</button><button class="icon-button" type="button" data-action="delete-class" data-id="${item.id}" aria-label="Delete class ${escapeHtml(item.sectionCode)}">${icons.trash}</button>` : ""}</div></td>
  </tr>`,
    )
    .join("");
  return `${heading("Classes", "Manage course sections, teachers, and rosters.", actionButton("Add class"))}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Class sections</h2><p class="panel-description">${state.classes.length} class section${state.classes.length === 1 ? "" : "s"}</p></div></div></div>
    <div class="table-wrap"><table><thead><tr><th>Course</th><th>Section</th><th>Term</th><th>Teacher</th><th>Schedule</th><th>Room</th><th>Students</th><th>Actions</th></tr></thead><tbody>${rows || '<tr><td class="table-empty" colspan="8">No classes found. Admins can add a class section.</td></tr>'}</tbody></table></div></section>`;
}

function renderRoster() {
  const classItem = state.classes.find(
    (item) => String(item.id) === String(state.rosterClassId),
  );
  if (!classItem)
    return `${heading("Class roster", "Class not found.")}<button class="button" data-action="back-to-classes">Back to classes</button>`;
  const rows = state.roster
    .map(
      (student) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(student.studentName)}</div><div class="secondary-cell">${escapeHtml(student.studentCode)}</div></td>
    <td>${escapeHtml(formatDate(student.enrolledAtUtc))}</td>
    ${state.user.role === "Admin" ? `<td><div class="table-actions"><button class="icon-button" type="button" data-action="remove-roster" data-id="${student.studentId}" aria-label="Remove ${escapeHtml(student.studentName)} from class">${icons.trash}</button></div></td>` : ""}
  </tr>`,
    )
    .join("");
  const addButton =
    state.user.role === "Admin"
      ? '<button class="button primary" type="button" data-action="add-roster"><span class="button-icon">' +
        icons.plus +
        "</span>Add student</button>"
      : "";
  return `${heading(`${escapeHtml(classItem.sectionCode)} roster`, `${escapeHtml(classItem.courseName)} · ${escapeHtml(classItem.term)}`, addButton)}
    <section class="panel table-panel"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Enrolled students</h2><p class="panel-description">${state.roster.length} student${state.roster.length === 1 ? "" : "s"}</p></div><button class="button small" type="button" data-action="back-to-classes">Back to classes</button></div></div>
    <div class="table-wrap"><table><thead><tr><th>Student</th><th>Enrolled</th>${state.user.role === "Admin" ? '<th><span class="sr-only">Actions</span></th>' : ""}</tr></thead><tbody>${rows || `<tr><td class="table-empty" colspan="${state.user.role === "Admin" ? 3 : 2}">This class roster is empty.</td></tr>`}</tbody></table></div></section>`;
}

function renderAttendance() {
  const classSelect = `<select id="attendance-class" aria-label="Class">${classOptions(state.selectedClassId)}</select>`;
  const rows = state.attendance
    .map(
      (item) => `<tr class="searchable">
    <td><div class="primary-cell">${escapeHtml(item.studentName)}</div><div class="secondary-cell">${escapeHtml(item.studentCode)}</div></td>
    <td><select class="attendance-status" data-student-id="${item.studentId}" aria-label="Attendance status for ${escapeHtml(item.studentName)}">
      ${["Present", "Absent", "Late", "Excused"].map((status) => `<option${(item.status || "Present") === status ? " selected" : ""}>${status}</option>`).join("")}
    </select></td>
    <td><input class="attendance-notes" data-student-id="${item.studentId}" aria-label="Notes for ${escapeHtml(item.studentName)}" maxlength="500" placeholder="Optional note" value="${escapeHtml(item.notes || "")}"></td>
  </tr>`,
    )
    .join("");
  return `${heading("Attendance", "Take attendance by class and date.")}
    <section class="panel"><div class="filters-row"><div class="field"><label for="attendance-class">Class</label>${classSelect}</div><div class="field"><label for="attendance-date">Date</label><input id="attendance-date" type="date" value="${escapeHtml(state.attendanceDate)}"></div><button class="button" type="button" data-action="load-attendance">Load roster</button>${state.classes.length ? '<button class="button primary" type="button" data-action="save-attendance">Save attendance</button>' : ""}</div></section>
    <section class="panel table-panel" style="margin-top:16px"><div class="table-wrap"><table><thead><tr><th>Student</th><th>Status</th><th>Notes</th></tr></thead><tbody>${rows || `<tr><td class="table-empty" colspan="3">${state.classes.length ? "No students are enrolled in this class." : "Create a class and enroll students before taking attendance."}</td></tr>`}</tbody></table></div></section>`;
}

function renderGrades() {
  const select = `<select id="grades-class" aria-label="Class">${classOptions(state.selectedClassId)}</select>`;
  const addAssessment = state.classes.length
    ? '<button class="button primary" type="button" data-action="create-assessment"><span class="button-icon">' +
      icons.plus +
      "</span>Add assessment</button>"
    : "";
  const assessments = state.assessments
    .map(
      (assessment) => `<article class="panel assessment-card">
    <div class="panel-heading"><div><h2 class="panel-title">${escapeHtml(assessment.title)}</h2><p class="panel-description">${assessment.pointsPossible} points${assessment.dueDate ? ` · Due ${escapeHtml(formatDate(assessment.dueDate))}` : ""}</p></div>${state.user.role === "Admin" ? `<button class="icon-button" type="button" data-action="delete-assessment" data-id="${assessment.id}" aria-label="Delete ${escapeHtml(assessment.title)}">${icons.trash}</button>` : ""}</div>
    <form class="grades-form" data-assessment-id="${assessment.id}"><div class="table-wrap"><table><thead><tr><th>Student</th><th>Score / ${assessment.pointsPossible}</th><th>Percent</th><th>Grade</th></tr></thead><tbody>${assessment.grades.map((grade) => `<tr class="searchable"><td><div class="primary-cell">${escapeHtml(grade.studentName)}</div><div class="secondary-cell">${escapeHtml(grade.studentCode)}</div></td><td><input class="grade-score" type="number" min="0" max="${assessment.pointsPossible}" step="0.01" data-student-id="${grade.studentId}" aria-label="Score for ${escapeHtml(grade.studentName)}" value="${grade.score ?? ""}" placeholder="Not graded"></td><td>${grade.percentage == null ? "—" : `${grade.percentage}%`}</td><td>${escapeHtml(grade.letterGrade || "—")}</td></tr>`).join("") || '<tr><td class="table-empty" colspan="4">No students are enrolled in this class.</td></tr>'}</tbody></table></div><div class="form-actions"><button class="button primary" type="submit">Save grades</button></div></form>
  </article>`,
    )
    .join("");
  return `${heading("Grades", "Create assessments and record scores.")}<section class="panel"><div class="filters-row"><div class="field"><label for="grades-class">Class</label>${select}</div>${addAssessment}</div></section><div class="assessment-list">${assessments || `<section class="panel table-empty">${state.classes.length ? "No assessments yet. Add one to start recording grades." : "Create a class and enroll students before grading."}</section>`}</div><p class="page-subtitle grade-scale">Letter grades: A ≥ 90%, B ≥ 80%, C ≥ 70%, D ≥ 60%, F below 60%.</p>`;
}

function renderReports() {
  const reportType = document.querySelector("#report-type")?.value || "roster";
  const selectedClass =
    document.querySelector("#report-class")?.value ||
    state.selectedClassId ||
    state.classes[0]?.id ||
    "";
  const filters = `<div class="field"><label for="report-type">Report</label><select id="report-type"><option value="roster"${reportType === "roster" ? " selected" : ""}>Class roster</option><option value="attendance"${reportType === "attendance" ? " selected" : ""}>Attendance</option><option value="grades"${reportType === "grades" ? " selected" : ""}>Grades</option></select></div>
    <div class="field"><label for="report-class">Class</label><select id="report-class">${classOptions(selectedClass)}</select></div>
    <div class="field report-date"${reportType !== "attendance" ? " hidden" : ""}><label for="report-from">From</label><input id="report-from" type="date" value="${escapeHtml(document.querySelector("#report-from")?.value || state.attendanceDate)}"></div>
    <div class="field report-date"${reportType !== "attendance" ? " hidden" : ""}><label for="report-to">To</label><input id="report-to" type="date" value="${escapeHtml(document.querySelector("#report-to")?.value || state.attendanceDate)}"></div>
    <button class="button primary" type="button" data-action="run-report">Run report</button>`;
  const visibleRows = state.reportRows.length
    ? `<div class="table-wrap"><table><thead><tr>${Object.keys(
        state.reportRows[0],
      )
        .map((key) => `<th>${escapeHtml(key)}</th>`)
        .join("")}</tr></thead><tbody>${state.reportRows
        .slice(0, 100)
        .map(
          (row) =>
            `<tr>${Object.values(row)
              .map((value) => `<td>${escapeHtml(value ?? "—")}</td>`)
              .join("")}</tr>`,
        )
        .join(
          "",
        )}</tbody></table></div>${state.reportRows.length > 100 ? `<p class="page-subtitle" style="padding:12px">Showing first 100 of ${state.reportRows.length} rows. CSV contains all rows.</p>` : ""}`
    : '<div class="table-empty">Choose a class and report, then run it to preview and export results.</div>';
  return `${heading("Reports", "Filter class data and export records as CSV.")}
    <section class="panel"><div class="filters-row">${filters}</div></section>
    <section class="panel table-panel" style="margin-top:16px"><div class="table-toolbar"><div class="panel-heading"><div><h2 class="panel-title">Report results</h2><p class="panel-description">${state.reportRows.length} row${state.reportRows.length === 1 ? "" : "s"}</p></div></div>${state.reportRows.length ? '<button class="button" type="button" data-action="download-report">Download CSV</button>' : ""}</div>${visibleRows}</section>`;
}

function renderSettings() {
  const timeZones = [
    ...new Set([
      state.settings.timeZone,
      "UTC",
      "Eastern Standard Time",
      "Central Standard Time",
      "Mountain Standard Time",
      "Pacific Standard Time",
      "GMT Standard Time",
      "W. Europe Standard Time",
      "SE Asia Standard Time",
      "Tokyo Standard Time",
      "AUS Eastern Standard Time",
    ]),
  ];
  const profile = `<form class="settings-form" data-kind="update-profile"><div class="form-grid">${field("fullName", "Full name", "text", state.user.fullName, { maxLength: 120 })}${field("email", "Email address", "email", state.user.email, { maxLength: 200 })}</div><div class="form-actions"><button class="button primary" type="submit">Save profile</button></div></form>`;
  const school =
    state.user.role === "Admin"
      ? `<section class="panel settings-section"><div class="panel-heading"><div><h2 class="panel-title">School settings</h2><p class="panel-description">These details apply to the school workspace.</p></div></div><form class="settings-form" data-kind="update-settings"><div class="form-grid">${field("schoolName", "School name", "text", state.settings.schoolName, { maxLength: 150 })}${selectField(
          "timeZone",
          "Time zone",
          timeZones.map((zone) => ({ value: zone, label: zone })),
          state.settings.timeZone,
        )}</div><div class="form-actions"><button class="button primary" type="submit">Save school settings</button></div></form></section>`
      : `<section class="panel settings-section"><div class="panel-heading"><div><h2 class="panel-title">School settings</h2><p class="panel-description">${escapeHtml(state.settings.schoolName)} · ${escapeHtml(state.settings.timeZone)}</p></div></div><p class="page-subtitle">Only admins can change school-wide settings.</p></section>`;
  return `${heading("Settings", "Manage your account and school preferences.")}
    <section class="panel settings-section"><div class="panel-heading"><div><h2 class="panel-title">Your profile</h2><p class="panel-description">Update the name and email address associated with your account.</p></div></div>${profile}</section>
    <section class="panel settings-section"><div class="panel-heading"><div><h2 class="panel-title">Change password</h2><p class="panel-description">Use at least 8 characters for your new password.</p></div></div><form class="settings-form" data-kind="change-password"><div class="form-grid">${field("currentPassword", "Current password", "password")}${field("newPassword", "New password", "password")}</div><div class="form-actions"><button class="button primary" type="submit">Change password</button></div></form></section>${school}`;
}

function applySearch() {
  const query = state.query.trim().toLocaleLowerCase();
  document.querySelectorAll(".searchable").forEach((row) => {
    row.hidden =
      query.length > 0 && !row.textContent.toLocaleLowerCase().includes(query);
  });
}
