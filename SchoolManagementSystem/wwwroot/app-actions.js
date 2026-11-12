async function submitRecord(form) {
  const values = Object.fromEntries(new FormData(form).entries());
  const kind = form.dataset.kind;
  const id = form.dataset.id;
  const modalError = form.querySelector(".modal-error");
  const submit = form.querySelector('button[type="submit"]');
  const verb = kind.startsWith("update") ? "PUT" : "POST";
  let path;
  let body = values;
  if (kind.endsWith("student")) {
    path = kind === "create-student" ? "/api/students" : `/api/students/${id}`;
    if (kind === "create-student") body.dateOfBirth = values.dateOfBirth;
  } else if (kind.endsWith("course")) {
    path = kind === "create-course" ? "/api/courses" : `/api/courses/${id}`;
    body.credits = Number(values.credits);
  } else if (kind.endsWith("class")) {
    path = kind === "create-class" ? "/api/classes" : `/api/classes/${id}`;
    body.courseId = Number(values.courseId);
    body.teacherId = Number(values.teacherId);
  } else if (kind === "create-teacher") {
    path = "/api/teachers";
  } else if (kind === "create-roster") {
    path = `/api/classes/${id}/roster`;
    body.studentId = Number(values.studentId);
  } else if (kind === "create-assessment") {
    path = `/api/classes/${id}/assessments`;
    body.pointsPossible = Number(values.pointsPossible);
    body.dueDate = values.dueDate || null;
  } else {
    const classSectionId = Number(values.classSectionId);
    const classItem = state.classes.find((item) => item.id === classSectionId);
    path = "/api/enrollments";
    body = {
      studentId: Number(values.studentId),
      courseId: classItem.courseId,
      classSectionId,
    };
  }
  submit.disabled = true;
  submit.textContent = "Saving...";
  modalError.hidden = true;
  try {
    await request(path, { method: verb, body: JSON.stringify(body) });
    document.querySelector("#record-modal").close();
    toast(kind.startsWith("update") ? "Changes saved." : "Record added.");
    await loadView();
  } catch (error) {
    modalError.textContent = errorMessage(error);
    modalError.hidden = false;
    submit.disabled = false;
    submit.textContent = kind.startsWith("update")
      ? "Save changes"
      : "Add record";
  }
}

async function submitSettings(form) {
  const values = Object.fromEntries(new FormData(form).entries());
  const kind = form.dataset.kind;
  const submit = form.querySelector('button[type="submit"]');
  submit.disabled = true;
  submit.textContent = "Saving...";
  try {
    if (kind === "update-profile") {
      const updated = await request("/api/auth/profile", {
        method: "PUT",
        body: JSON.stringify(values),
      });
      state.user = {
        fullName: updated.fullName,
        email: updated.email,
        role: updated.role,
      };
      state.token = updated.token;
      sessionStorage.setItem("educloud.user", JSON.stringify(state.user));
      sessionStorage.setItem("educloud.token", state.token);
    } else if (kind === "change-password") {
      await request("/api/auth/password", {
        method: "PUT",
        body: JSON.stringify(values),
      });
      form.reset();
    } else {
      await request("/api/settings", {
        method: "PUT",
        body: JSON.stringify(values),
      });
      state.settings = {
        schoolName: values.schoolName,
        timeZone: values.timeZone,
      };
    }
    toast("Settings saved.");
    renderShell();
    await loadView();
  } catch (error) {
    toast(errorMessage(error), true);
    submit.disabled = false;
    submit.textContent =
      kind === "update-profile"
        ? "Save profile"
        : kind === "change-password"
          ? "Change password"
          : "Save school settings";
  }
}

async function saveAttendance() {
  const classId = document.querySelector("#attendance-class")?.value;
  const date = document.querySelector("#attendance-date")?.value;
  if (!classId || !date) {
    toast("Choose a class and date.", true);
    return;
  }
  const records = [...document.querySelectorAll(".attendance-status")].map(
    (select) => ({
      studentId: Number(select.dataset.studentId),
      status: select.value,
      notes:
        document.querySelector(
          `.attendance-notes[data-student-id="${select.dataset.studentId}"]`,
        )?.value || null,
    }),
  );
  if (!records.length) {
    toast("There are no enrolled students to mark.", true);
    return;
  }
  try {
    await request(`/api/classes/${classId}/attendance`, {
      method: "PUT",
      body: JSON.stringify({ attendanceDate: date, records }),
    });
    state.attendanceDate = date;
    state.selectedClassId = classId;
    toast("Attendance saved.");
    await loadView();
  } catch (error) {
    toast(errorMessage(error), true);
  }
}

async function saveGrades(form) {
  const assessmentId = form.dataset.assessmentId;
  const grades = [...form.querySelectorAll(".grade-score")].map((input) => ({
    studentId: Number(input.dataset.studentId),
    score: input.value === "" ? null : Number(input.value),
  }));
  if (!grades.length) {
    toast("There are no enrolled students to grade.", true);
    return;
  }
  const button = form.querySelector('button[type="submit"]');
  button.disabled = true;
  button.textContent = "Saving...";
  try {
    await request(`/api/assessments/${assessmentId}/grades`, {
      method: "PUT",
      body: JSON.stringify({ grades }),
    });
    toast("Grades saved.");
    await loadView();
  } catch (error) {
    toast(errorMessage(error), true);
    button.disabled = false;
    button.textContent = "Save grades";
  }
}

async function runReport() {
  const type = document.querySelector("#report-type").value;
  const classId = document.querySelector("#report-class").value;
  if (!classId) {
    toast("Choose a class before running this report.", true);
    return;
  }
  let path = `/api/reports/${type}?classId=${encodeURIComponent(classId)}`;
  if (type === "attendance") {
    const from = document.querySelector("#report-from").value;
    const to = document.querySelector("#report-to").value;
    if (!from || !to) {
      toast("Choose both dates for the attendance report.", true);
      return;
    }
    path += `&from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`;
  }
  try {
    state.reportRows = await request(path);
    state.selectedClassId = classId;
    const content = document.querySelector("#page-content");
    content.innerHTML = renderReports();
    applySearch();
  } catch (error) {
    toast(errorMessage(error), true);
  }
}

function downloadReport() {
  if (!state.reportRows.length) return;
  const columns = Object.keys(state.reportRows[0]);
  const csvCell = (value) => {
    let text = String(value ?? "");
    if (/^[=+\-@]/.test(text)) text = `'${text}`;
    return `"${text.replaceAll('"', '""')}"`;
  };
  const content = [
    columns.map(csvCell).join(","),
    ...state.reportRows.map((row) =>
      columns.map((column) => csvCell(row[column])).join(","),
    ),
  ].join("\r\n");
  const blob = new Blob(["\uFEFF", content], {
    type: "text/csv;charset=utf-8",
  });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `educloud-${document.querySelector("#report-type").value}-${localDateValue()}.csv`;
  link.style.position = "fixed";
  link.style.left = "-10000px";
  document.body.append(link);
  link.click();
  link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

async function deleteRecord(kind, id, name) {
  const warning =
    kind === "class"
      ? `Delete ${name}? Its roster, attendance history, assessments, and grades will also be deleted.`
      : `Delete ${name}? This action cannot be undone.`;
  if (!window.confirm(warning)) return;
  const paths = {
    student: `/api/students/${id}`,
    course: `/api/courses/${id}`,
    enrollment: `/api/enrollments/${id}`,
    class: `/api/classes/${id}`,
    assessment: `/api/assessments/${id}`,
  };
  try {
    await request(paths[kind], { method: "DELETE" });
    toast("Record deleted.");
    await loadView();
  } catch (error) {
    toast(errorMessage(error), true);
  }
}
