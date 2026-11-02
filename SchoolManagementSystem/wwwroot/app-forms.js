function field(name, label, type = "text", value = "", options = {}) {
  const id = `field-${name}`;
  const required = options.required === false ? "" : " required";
  const full = options.full ? " full" : "";
  const maxLength = options.maxLength
    ? ` maxlength="${options.maxLength}"`
    : "";
  const inputValue = escapeHtml(value);
  let control;
  if (type === "textarea") {
    control = `<textarea id="${id}" name="${name}"${required}${maxLength}>${inputValue}</textarea>`;
  } else if (type === "select") {
    const selected = options.options
      .map(
        (option) =>
          `<option value="${escapeHtml(option)}"${option === value ? " selected" : ""}>${escapeHtml(option)}</option>`,
      )
      .join("");
    control = `<select id="${id}" name="${name}"${required}>${selected}</select>`;
  } else {
    control = `<input id="${id}" name="${name}" type="${type}" value="${inputValue}"${required}${maxLength}${options.min ? ` min="${options.min}"` : ""}${options.max ? ` max="${options.max}"` : ""}${options.step ? ` step="${options.step}"` : ""}>`;
  }
  return `<div class="field${full}"><label for="${id}">${label}</label>${control}</div>`;
}

function openModal(title, description, body, submitLabel) {
  const dialog = document.querySelector("#record-modal");
  dialog.innerHTML = `<form class="modal-form" id="record-form"><div class="modal-header"><div><h2 class="modal-title" id="modal-title">${title}</h2><p class="modal-description">${description}</p></div><button class="icon-button" type="button" data-action="close-modal" aria-label="Close">${icons.close}</button></div><div class="modal-body">${body}<div class="notice modal-error" role="alert" hidden></div><div class="modal-actions"><button class="button" type="button" data-action="close-modal">Cancel</button><button class="button primary" type="submit">${submitLabel}</button></div></div></form>`;
  dialog.showModal();
}

function studentForm(student) {
  const update = Boolean(student);
  const body = `<div class="form-grid">
    ${update ? "" : field("studentCode", "Student code", "text", "", { maxLength: 30 })}
    ${field("firstName", "First name", "text", student?.firstName || "", { maxLength: 100 })}
    ${field("lastName", "Last name", "text", student?.lastName || "", { maxLength: 100 })}
    ${field("dateOfBirth", "Date of birth", "date", student?.dateOfBirth || "")}
    ${field("gender", "Gender", "select", student?.gender || "Female", { options: ["Female", "Male", "Other"] })}
    ${field("email", "Email address", "email", student?.email || "", { maxLength: 200 })}
    ${field("phone", "Phone (optional)", "tel", student?.phone || "", { required: false })}
    ${field("address", "Address (optional)", "textarea", student?.address || "", { required: false, full: true })}
  </div>`;
  openModal(
    update ? "Edit student" : "Add student",
    update
      ? "Update the student record."
      : "Enter student details to add a record.",
    body,
    update ? "Save changes" : "Add student",
  );
  document.querySelector("#record-form").dataset.kind = update
    ? "update-student"
    : "create-student";
  if (update) document.querySelector("#record-form").dataset.id = student.id;
}

function courseForm(course) {
  const update = Boolean(course);
  const body = `<div class="form-grid">
    ${update ? "" : field("courseCode", "Course code", "text", "", { maxLength: 30 })}
    ${field("name", "Course name", "text", course?.name || "", { maxLength: 150 })}
    ${field("credits", "Credits", "number", course?.credits ?? 3, { min: "1", max: "30" })}
    ${field("description", "Description (optional)", "textarea", course?.description || "", { required: false, full: true })}
  </div>`;
  openModal(
    update ? "Edit course" : "Add course",
    update ? "Update course information." : "Add a course to the catalog.",
    body,
    update ? "Save changes" : "Add course",
  );
  document.querySelector("#record-form").dataset.kind = update
    ? "update-course"
    : "create-course";
  if (update) document.querySelector("#record-form").dataset.id = course.id;
}

function enrollmentForm() {
  if (!state.students.length || !state.courses.length) {
    toast(
      "Add at least one student and one course before creating an enrollment.",
      true,
    );
    return;
  }
  const studentOptions = state.students
    .map(
      (student) =>
        `<option value="${student.id}">${escapeHtml(student.firstName)} ${escapeHtml(student.lastName)} (${escapeHtml(student.studentCode)})</option>`,
    )
    .join("");
  const courseOptions = state.courses
    .map(
      (course) =>
        `<option value="${course.id}">${escapeHtml(course.name)} (${escapeHtml(course.courseCode)})</option>`,
    )
    .join("");
  const body = `<div class="form-grid"><div class="field full"><label for="field-studentId">Student</label><select id="field-studentId" name="studentId" required>${studentOptions}</select></div><div class="field full"><label for="field-courseId">Course</label><select id="field-courseId" name="courseId" required>${courseOptions}</select></div></div>`;
  openModal(
    "Add enrollment",
    "Choose a student and course to register.",
    body,
    "Add enrollment",
  );
  document.querySelector("#record-form").dataset.kind = "create-enrollment";
}

function teacherForm() {
  const body = `<div class="form-grid">${field("fullName", "Full name", "text", "", { maxLength: 120 })}${field("email", "Email address", "email", "", { maxLength: 200 })}${field("temporaryPassword", "Temporary password", "password", "", { min: "8", full: true })}</div><p class="page-subtitle">The teacher can change this password from Settings.</p>`;
  openModal(
    "Add teacher",
    "Create a Teacher account for a staff member.",
    body,
    "Create teacher",
  );
  document.querySelector("#record-form").dataset.kind = "create-teacher";
}

function classForm(classItem) {
  const update = Boolean(classItem);
  const courseChoices = state.courses.map((course) => ({
    value: course.id,
    label: `${course.courseCode} · ${course.name}`,
  }));
  const teacherChoices = state.teachers.map((teacher) => ({
    value: teacher.id,
    label: `${teacher.fullName} · ${teacher.email}`,
  }));
  if (!courseChoices.length || !teacherChoices.length) {
    toast("Add a course and a teacher account before creating a class.", true);
    return;
  }
  const course = selectField(
    "courseId",
    "Course",
    courseChoices,
    classItem?.courseId ?? courseChoices[0].value,
  );
  const teacher = selectField(
    "teacherId",
    "Assigned teacher",
    teacherChoices,
    classItem?.teacherId ?? teacherChoices[0].value,
  );
  const code = update
    ? ""
    : field("sectionCode", "Section code", "text", "", { maxLength: 30 });
  const body = `<div class="form-grid">${course}${teacher}${code}${field("term", "Term", "text", classItem?.term || "", { maxLength: 100 })}${field("schedule", "Schedule (optional)", "text", classItem?.schedule || "", { required: false, maxLength: 200 })}${field("room", "Room (optional)", "text", classItem?.room || "", { required: false, maxLength: 100 })}</div>`;
  openModal(
    update ? "Edit class" : "Add class",
    "Create a section for a course and assign its teacher.",
    body,
    update ? "Save changes" : "Add class",
  );
  document.querySelector("#record-form").dataset.kind = update
    ? "update-class"
    : "create-class";
  if (update) document.querySelector("#record-form").dataset.id = classItem.id;
}

function rosterForm() {
  const enrolled = new Set(state.roster.map((student) => student.studentId));
  const available = state.students.filter(
    (student) => !enrolled.has(student.id),
  );
  if (!available.length) {
    toast("All students are already enrolled in this class.", true);
    return;
  }
  const choices = available
    .map(
      (student) =>
        `<option value="${student.id}">${escapeHtml(student.firstName)} ${escapeHtml(student.lastName)} (${escapeHtml(student.studentCode)})</option>`,
    )
    .join("");
  openModal(
    "Add student to class",
    "Choose a student for this class roster.",
    `<div class="form-grid"><div class="field full"><label for="field-studentId">Student</label><select id="field-studentId" name="studentId" required>${choices}</select></div></div>`,
    "Add student",
  );
  const form = document.querySelector("#record-form");
  form.dataset.kind = "create-roster";
  form.dataset.id = state.rosterClassId;
}

function assessmentForm() {
  const body = `<div class="form-grid">${field("title", "Assessment title", "text", "", { maxLength: 150 })}${field("pointsPossible", "Points possible", "number", "100", { min: "0.01", max: "1000000", step: "0.01" })}${field("dueDate", "Due date (optional)", "date", "", { required: false, full: true })}</div>`;
  openModal(
    "Add assessment",
    "Scores are converted to a percentage and letter grade.",
    body,
    "Create assessment",
  );
  const form = document.querySelector("#record-form");
  form.dataset.kind = "create-assessment";
  form.dataset.id = state.selectedClassId;
}

async function submitAuth(form) {
  const values = Object.fromEntries(new FormData(form).entries());
  const action = state.authMode === "register" ? "register" : "login";
  const error = document.querySelector(".auth-error");
  const submit = form.querySelector('button[type="submit"]');
  error.hidden = true;
  error.style.display = "none";
  submit.disabled = true;
  submit.textContent =
    state.authMode === "register" ? "Creating account..." : "Signing in...";
  try {
    const result = await request(`/api/auth/${action}`, {
      method: "POST",
      body: JSON.stringify(values),
    });
    state.token = result.token;
    state.user = {
      fullName: result.fullName,
      email: result.email,
      role: result.role,
    };
    sessionStorage.setItem("educloud.token", state.token);
    sessionStorage.setItem("educloud.user", JSON.stringify(state.user));
    state.view = "dashboard";
    state.query = "";
    render();
  } catch (exception) {
    error.textContent = errorMessage(exception);
    error.hidden = false;
    error.style.display = "block";
    submit.disabled = false;
    submit.textContent =
      state.authMode === "register" ? "Create account" : "Sign in";
  }
}
