app.addEventListener("click", async (event) => {
  const button = event.target.closest("button");
  if (!button) return;
  if (button.dataset.authMode) {
    state.authMode = button.dataset.authMode;
    renderAuth();
  } else if (button.dataset.view) {
    state.view = button.dataset.view;
    state.query = "";
    if (state.view !== "classes") state.rosterClassId = "";
    if (state.view !== "reports") state.reportRows = [];
    renderShell();
    loadView();
    document.querySelector("#sidebar").classList.remove("open");
  } else if (button.dataset.action === "menu") {
    document.querySelector("#sidebar").classList.toggle("open");
  } else if (button.dataset.action === "logout") {
    clearSession();
    state.query = "";
    renderAuth();
  } else if (button.dataset.action === "refresh") {
    loadView();
  } else if (button.dataset.action === "create") {
    if (state.view === "students") studentForm();
    else if (state.view === "courses") courseForm();
    else if (state.view === "teachers") teacherForm();
    else if (state.view === "classes") classForm();
    else if (state.view === "enrollments") enrollmentForm();
  } else if (button.dataset.action === "edit-student") {
    const student = state.students.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (student) studentForm(student);
  } else if (button.dataset.action === "edit-course") {
    const course = state.courses.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (course) courseForm(course);
  } else if (button.dataset.action === "edit-class") {
    const classItem = state.classes.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (classItem) classForm(classItem);
  } else if (button.dataset.action === "view-roster") {
    state.rosterClassId = button.dataset.id;
    await loadView();
  } else if (button.dataset.action === "back-to-classes") {
    state.rosterClassId = "";
    await loadView();
  } else if (button.dataset.action === "add-roster") {
    state.students = await request("/api/students");
    rosterForm();
  } else if (button.dataset.action === "remove-roster") {
    const student = state.roster.find(
      (item) => item.studentId === Number(button.dataset.id),
    );
    if (
      student &&
      window.confirm(`Remove ${student.studentName} from this class?`)
    ) {
      try {
        await request(
          `/api/classes/${state.rosterClassId}/roster/${student.studentId}`,
          { method: "DELETE" },
        );
        toast("Student removed from class.");
        await loadView();
      } catch (error) {
        toast(errorMessage(error), true);
      }
    }
  } else if (button.dataset.action === "delete-class") {
    const classItem = state.classes.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (classItem)
      await deleteRecord(
        "class",
        classItem.id,
        `${classItem.sectionCode} class`,
      );
  } else if (button.dataset.action === "delete-assessment") {
    const assessment = state.assessments.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (assessment)
      await deleteRecord("assessment", assessment.id, assessment.title);
  } else if (button.dataset.action === "create-assessment") {
    assessmentForm();
  } else if (button.dataset.action === "load-attendance") {
    const classId = document.querySelector("#attendance-class")?.value;
    const date = document.querySelector("#attendance-date")?.value;
    if (classId && date) {
      state.selectedClassId = classId;
      state.attendanceDate = date;
      await loadView();
    }
  } else if (button.dataset.action === "save-attendance") {
    await saveAttendance();
  } else if (button.dataset.action === "run-report") {
    await runReport();
  } else if (button.dataset.action === "download-report") {
    downloadReport();
  } else if (button.dataset.action === "delete-student") {
    const student = state.students.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (student)
      await deleteRecord(
        "student",
        student.id,
        `${student.firstName} ${student.lastName}`,
      );
  } else if (button.dataset.action === "delete-course") {
    const course = state.courses.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (course) await deleteRecord("course", course.id, course.name);
  } else if (button.dataset.action === "delete-enrollment") {
    const enrollment = state.enrollments.find(
      (item) => item.id === Number(button.dataset.id),
    );
    if (enrollment)
      await deleteRecord(
        "enrollment",
        enrollment.id,
        `${enrollment.studentName}'s enrollment in ${enrollment.courseName}`,
      );
  } else if (button.dataset.action === "close-modal") {
    document.querySelector("#record-modal").close();
  }
});

app.addEventListener("submit", async (event) => {
  if (event.target.id === "auth-form") {
    event.preventDefault();
    await submitAuth(event.target);
  } else if (event.target.id === "record-form") {
    event.preventDefault();
    await submitRecord(event.target);
  } else if (event.target.matches(".settings-form")) {
    event.preventDefault();
    await submitSettings(event.target);
  } else if (event.target.matches(".grades-form")) {
    event.preventDefault();
    await saveGrades(event.target);
  }
});

app.addEventListener("input", (event) => {
  if (event.target.id === "global-search") {
    state.query = event.target.value;
    applySearch();
  }
});

app.addEventListener("change", async (event) => {
  if (
    event.target.id === "attendance-class" ||
    event.target.id === "attendance-date"
  ) {
    const classId = document.querySelector("#attendance-class")?.value;
    const date = document.querySelector("#attendance-date")?.value;
    if (classId && date) {
      state.selectedClassId = classId;
      state.attendanceDate = date;
      await loadView();
    }
  } else if (event.target.id === "grades-class") {
    state.selectedClassId = event.target.value;
    await loadView();
  } else if (event.target.id === "report-type") {
    state.reportRows = [];
    const content = document.querySelector("#page-content");
    content.innerHTML = renderReports();
  } else if (event.target.id === "report-class") {
    state.selectedClassId = event.target.value;
  }
});

app.addEventListener("keydown", (event) => {
  if (event.key === "Escape")
    document.querySelector("#sidebar")?.classList.remove("open");
});

render();
