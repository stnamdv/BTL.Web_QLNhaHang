// Order Workflow Management
class OrderWorkflow {
  constructor() {
    this.orderId = null;
    this.currentStep = null;
    this.processingSteps = [];
    this.selectedEmployee = null;
    this.currentStepId = null;

    this.init();
  }

  init() {
    // Get order ID from URL or data attribute
    this.orderId = this.getOrderIdFromUrl();

    if (this.orderId) {
      this.loadProcessingSteps();
      this.setupEventListeners();
    }
  }

  getOrderIdFromUrl() {
    const path = window.location.pathname;
    const match = path.match(/\/Order\/Workflow\/(\d+)/);
    return match ? parseInt(match[1]) : null;
  }

  setupEventListeners() {
    // Employee selection modal events
    document
      .getElementById("confirmEmployeeSelection")
      ?.addEventListener("click", () => {
        this.confirmEmployeeSelection();
      });

    // Employee card selection
    document.addEventListener("click", (e) => {
      if (e.target.closest(".employee-card")) {
        this.selectEmployee(e.target.closest(".employee-card"));
      }
    });
  }

  async loadProcessingSteps() {
    try {
      console.log("Loading processing steps for order:", this.orderId);

      // Load processing steps
      const response = await fetch(`/BuocXuLy/GetAll`);
      if (!response.ok) throw new Error("Failed to load processing steps");

      this.processingSteps = await response.json();
      console.log("Processing steps loaded:", this.processingSteps);

      // Log step IDs for debugging
      if (this.processingSteps && this.processingSteps.length > 0) {
        console.log(
          "Step IDs:",
          this.processingSteps.map((s) => s.buoc_id)
        );
        console.log("First step:", this.processingSteps[0]);
        console.log("All steps:", this.processingSteps);

        // Check if any step has buoc_id = 0 or undefined
        const invalidSteps = this.processingSteps.filter(
          (s) => !s.buoc_id || s.buoc_id === 0
        );
        if (invalidSteps.length > 0) {
          console.error("Found steps with invalid buoc_id:", invalidSteps);
        }
      } else {
        console.error("No processing steps loaded!");
      }

      // Load current order status
      await this.loadOrderStatus();

      // Render timeline
      this.renderTimeline();
    } catch (error) {
      console.error("Error loading processing steps:", error);
      this.showError("Không thể tải quy trình xử lý");
    }
  }

  async loadOrderStatus() {
    try {
      const response = await fetch(
        `/LichSuThucHien/GetByOrder/${this.orderId}`
      );
      if (!response.ok) throw new Error("Failed to load order status");

      const result = await response.json();
      console.log("Order status loaded:", result);
      console.log("Order status data:", result.data);

      // Process order status to determine current step
      if (result.success && result.data) {
        console.log("Processing order status data:", result.data);
        this.processOrderStatus(result.data);
      } else {
        console.warn("No order status data available");
        this.processOrderStatus([]);
      }
    } catch (error) {
      console.error("Error loading order status:", error);
      // If no order status data, initialize with empty data to allow first step
      this.processOrderStatus([]);
    }
  }

  processOrderStatus(orderStatus) {
    console.log("=== processOrderStatus called ===");
    console.log("Input orderStatus:", orderStatus);

    // Group by step
    const stepStatus = {};

    orderStatus.forEach((item) => {
      console.log("Processing item:", item);
      if (!stepStatus[item.buoc_id]) {
        stepStatus[item.buoc_id] = {
          buoc_id: item.buoc_id,
          ten_buoc: item.ten_buoc,
          status: "CHUA_BAT_DAU",
          items: [],
          startTime: null,
          endTime: null,
          duration: null,
        };
      }
      stepStatus[item.buoc_id].items.push(item);

      // Determine overall step status and track times
      if (item.trang_thai === "HOAN_THANH") {
        stepStatus[item.buoc_id].status = "HOAN_THANH";
        if (item.thoi_diem_ket_thuc) {
          stepStatus[item.buoc_id].endTime = item.thoi_diem_ket_thuc;
        }
      } else if (item.trang_thai === "DANG_THUC_HIEN") {
        stepStatus[item.buoc_id].status = "DANG_THUC_HIEN";
        if (item.thoi_diem_bat_dau) {
          stepStatus[item.buoc_id].startTime = item.thoi_diem_bat_dau;
        }
      }

      // Store employee info
      if (item.nv_id && item.nv_ho_ten) {
        stepStatus[item.buoc_id].employeeId = item.nv_id;
        stepStatus[item.buoc_id].employeeName = item.nv_ho_ten;
        stepStatus[item.buoc_id].employeeRole = item.loai_nv || "";
      }

      // Set start time if not already set
      if (item.thoi_diem_bat_dau && !stepStatus[item.buoc_id].startTime) {
        stepStatus[item.buoc_id].startTime = item.thoi_diem_bat_dau;
      }
    });

    // Calculate duration for completed steps
    Object.values(stepStatus).forEach((step) => {
      if (step.startTime && step.endTime) {
        step.duration = this.calculateDuration(step.startTime, step.endTime);
      }
    });

    // Store step status for time info display
    this.stepStatusData = stepStatus;
    console.log("Final stepStatusData:", this.stepStatusData);

    // Find current step (first non-completed step)
    this.currentStep = null;
    this.currentStepId = null;

    // If no history data at all, make first step current
    if (Object.keys(stepStatus).length === 0) {
      if (this.processingSteps.length > 0) {
        this.currentStep = this.processingSteps[0];
        this.currentStepId = this.processingSteps[0].buoc_id;
        console.log(
          "No history data - setting first step as current:",
          this.currentStep
        );
      }
    } else {
      // Find first non-completed step
      for (const step of this.processingSteps) {
        const stepData = stepStatus[step.buoc_id];

        // If step is not completed, it's current
        if (!stepData || stepData.status !== "HOAN_THANH") {
          this.currentStep = step;
          this.currentStepId = step.buoc_id;
          break;
        }
      }
    }

    // Final fallback - ensure we always have a current step
    if (!this.currentStep && this.processingSteps.length > 0) {
      this.currentStep = this.processingSteps[0];
      this.currentStepId = this.processingSteps[0].buoc_id;
      console.log(
        "Fallback - setting first step as current:",
        this.currentStep
      );
    }

    console.log("Current step determined:", this.currentStep);
    console.log("Step status data:", this.stepStatusData);
  }

  renderTimeline() {
    const container = document.getElementById("processingSteps");
    if (!container) return;

    container.innerHTML = "";

    this.processingSteps.forEach((step, index) => {
      const stepElement = this.createStepElement(step, index);
      container.appendChild(stepElement);
    });

    // Start timers for active steps
    this.startTimers();
  }

  createStepElement(step, index) {
    const stepDiv = document.createElement("div");
    stepDiv.className = "timeline-step";
    stepDiv.dataset.stepId = step.buoc_id;

    console.log(
      `Creating step element for step ${step.buoc_id} (${step.ten_buoc})`
    );

    const status = this.getStepStatus(step.buoc_id);
    const isCurrent =
      this.currentStep && this.currentStep.buoc_id === step.buoc_id;
    const isCompleted = status === "HOAN_THANH";
    const isPending = status === "CHUA_BAT_DAU";
    const isFirstStep = index === 0;

    // Get time information for this step
    const timeInfo = this.getStepTimeInfo(step, status);

    stepDiv.innerHTML = `
            <div class="step-circle ${
              isCompleted ? "completed" : isCurrent ? "current" : "pending"
            }">
                ${isCompleted ? '<i class="fas fa-check"></i>' : index + 1}
            </div>
            <div class="step-content">
                <div class="step-header">
                    <h4 class="step-title">${step.ten_buoc}</h4>
                    <span class="step-status ${
                      isCompleted
                        ? "completed"
                        : isCurrent
                        ? "current"
                        : "pending"
                    }">
                        ${this.getStatusText(status)}
                    </span>
                </div>
                ${
                  isFirstStep
                    ? '<div class="step-start-indicator"><i class="fas fa-flag"></i> Bước bắt đầu quy trình</div>'
                    : ""
                }
                <div class="step-description">
                    ${step.mo_ta || "Không có mô tả"}
                </div>
                ${isFirstStep ? this.createOrderDetailsStep() : ""}
                <div class="step-time-info">
                    ${timeInfo}
                </div>
                <div class="step-actions">
                    ${this.createStepActions(step, status, isCurrent)}
                </div>
            </div>
        `;

    // Add event listeners for step actions
    this.addStepActionListeners(stepDiv, step, status);

    return stepDiv;
  }

  getStepStatus(stepId) {
    // Check if we have step status data from order history
    if (this.stepStatusData && this.stepStatusData[stepId]) {
      return this.stepStatusData[stepId].status;
    }

    // If no history data, determine status based on step order
    const stepIndex = this.processingSteps.findIndex(
      (s) => s.buoc_id === stepId
    );

    // First step is always available to start
    if (stepIndex === 0) {
      return "CHUA_BAT_DAU";
    }

    // For other steps, check if previous steps are completed
    for (let i = 0; i < stepIndex; i++) {
      const prevStepId = this.processingSteps[i].buoc_id;
      const prevStepStatus = this.getStepStatus(prevStepId);
      if (prevStepStatus !== "HOAN_THANH") {
        return "CHUA_BAT_DAU"; // Wait for previous steps
      }
    }

    return "CHUA_BAT_DAU";
  }

  getStatusText(status) {
    const statusMap = {
      CHUA_BAT_DAU: "Chưa bắt đầu",
      DANG_THUC_HIEN: "Đang thực hiện",
      HOAN_THANH: "Hoàn thành",
    };
    return statusMap[status] || status;
  }

  createOrderDetailsStep() {
    // Get order details from the page
    const orderInfo = document.querySelector(".order-info");
    if (!orderInfo) return "";

    const orderId =
      orderInfo.querySelector("[data-order-id]")?.textContent || "";
    const orderTime =
      orderInfo.querySelector("[data-order-time]")?.textContent || "";
    const customerCount =
      orderInfo.querySelector("[data-customer-count]")?.textContent || "";
    const totalAmount =
      orderInfo.querySelector("[data-total-amount]")?.textContent || "";

    return `
      <div class="order-details-step">
        <h6><i class="fas fa-receipt"></i> Thông tin đơn hàng</h6>
        <div class="order-details-grid">
          <div class="detail-item">
            <span class="detail-label">Mã đơn:</span>
            <span class="detail-value">${orderId}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Thời gian đặt:</span>
            <span class="detail-value">${orderTime}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Số khách:</span>
            <span class="detail-value">${customerCount}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Tổng tiền:</span>
            <span class="detail-value">${totalAmount}</span>
          </div>
        </div>
      </div>
    `;
  }

  getStepTimeInfo(step, status) {
    let timeInfo = "";

    if (step.thoi_gian_du_kien) {
      timeInfo += `<div class="time-item">
        <i class="fas fa-clock"></i>
        <span>Thời gian dự kiến: ${step.thoi_gian_du_kien} phút</span>
      </div>`;
    }

    // Add actual time information if step is completed or in progress
    if (status === "DANG_THUC_HIEN" || status === "HOAN_THANH") {
      const stepHistory = this.getStepHistory(step.buoc_id);
      if (stepHistory) {
        if (stepHistory.startTime) {
          timeInfo += `<div class="time-item">
            <i class="fas fa-play"></i>
            <span>Bắt đầu: ${this.formatDateTime(stepHistory.startTime)}</span>
          </div>`;
        }
        if (stepHistory.endTime) {
          timeInfo += `<div class="time-item">
            <i class="fas fa-check"></i>
            <span>Hoàn thành: ${this.formatDateTime(stepHistory.endTime)}</span>
          </div>`;
        }
        if (stepHistory.duration) {
          timeInfo += `<div class="time-item">
            <i class="fas fa-stopwatch"></i>
            <span>Thời gian thực tế: ${stepHistory.duration}</span>
          </div>`;
        }
      }
    }

    return timeInfo;
  }

  getStepHistory(stepId) {
    // Get step history from the processed order status data
    if (this.stepStatusData && this.stepStatusData[stepId]) {
      return this.stepStatusData[stepId];
    }
    return null;
  }

  formatDateTime(dateTimeString) {
    if (!dateTimeString) return "";
    const date = new Date(dateTimeString);
    return date.toLocaleString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  calculateDuration(startTime, endTime) {
    if (!startTime || !endTime) return "Không xác định";
    const start = new Date(startTime);
    const end = new Date(endTime);
    const diffMs = end - start;

    if (diffMs < 0) return "Lỗi thời gian";

    const seconds = Math.floor(diffMs / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);

    if (hours > 0) {
      return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    } else {
      return `${seconds}s`;
    }
  }

  getStepEmployeeInfo(stepId) {
    // Get employee info from step status data
    if (this.stepStatusData && this.stepStatusData[stepId]) {
      const stepData = this.stepStatusData[stepId];
      return {
        name: stepData.employeeName || "Không xác định",
        role: stepData.employeeRole || "",
      };
    }
    return { name: "Đang tải...", role: "" };
  }

  getStepStartTime(stepId) {
    // Get start time from step status data
    if (this.stepStatusData && this.stepStatusData[stepId]) {
      return this.stepStatusData[stepId].startTime;
    }
    return null;
  }

  getStepEndTime(stepId) {
    // Get end time from step status data
    if (this.stepStatusData && this.stepStatusData[stepId]) {
      return this.stepStatusData[stepId].endTime;
    }
    return null;
  }

  startTimers() {
    // Clear existing timers
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }

    // Update timers every second
    this.timerInterval = setInterval(() => {
      this.updateElapsedTimes();
    }, 1000);

    // Initial update
    this.updateElapsedTimes();
  }

  updateElapsedTimes() {
    const elapsedElements = document.querySelectorAll(".elapsed-time");
    elapsedElements.forEach((element) => {
      const startTimeStr = element.getAttribute("data-start-time");
      if (startTimeStr) {
        const startTime = new Date(startTimeStr);
        const now = new Date();
        const elapsedMs = now - startTime;

        if (elapsedMs > 0) {
          element.textContent = this.formatElapsedTime(elapsedMs);
        }
      }
    });
  }

  formatElapsedTime(milliseconds) {
    const seconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);

    if (hours > 0) {
      return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    } else {
      return `${seconds}s`;
    }
  }

  createStepActions(step, status, isCurrent) {
    let actions = "";
    const stepIndex = this.processingSteps.findIndex(
      (s) => s.buoc_id === step.buoc_id
    );
    const isFirstStep = stepIndex === 0;

    console.log(
      `Creating actions for step ${step.buoc_id}: status=${status}, isCurrent=${isCurrent}, isFirstStep=${isFirstStep}`
    );

    if (status === "CHUA_BAT_DAU" && (isCurrent || isFirstStep)) {
      console.log(
        `Creating start button for step ${step.buoc_id} (${step.ten_buoc})`
      );
      actions += `
                <button class="btn-step btn-start" data-action="start" data-step-id="${
                  step.buoc_id
                }">
                    <i class="fas fa-play"></i> Bắt đầu xử lý
                </button>
                <div class="step-note">
                    <small class="text-muted">
                        <i class="fas fa-info-circle"></i> 
                        ${
                          isFirstStep
                            ? 'Bước đầu tiên - Nhấn "Bắt đầu" để bắt đầu quy trình xử lý'
                            : 'Nhấn "Bắt đầu" để chọn nhân viên và bắt đầu xử lý bước này'
                        }
                    </small>
                </div>
            `;
    } else if (status === "DANG_THUC_HIEN") {
      // Get employee info for this step
      const employeeInfo = this.getStepEmployeeInfo(step.buoc_id);
      const startTime = this.getStepStartTime(step.buoc_id);

      actions += `
                <div class="step-employee-info mb-3">
                    <div class="d-flex align-items-center">
                        <i class="fas fa-user-circle text-primary me-2"></i>
                        <div>
                            <strong>Người xử lý:</strong> ${
                              employeeInfo.name || "Đang tải..."
                            }
                            <br>
                            <small class="text-muted">${
                              employeeInfo.role || ""
                            }</small>
                        </div>
                    </div>
                </div>
                <div class="step-timer mb-3">
                    <div class="d-flex align-items-center">
                        <i class="fas fa-stopwatch text-warning me-2"></i>
                        <div>
                            <strong>Thời gian bắt đầu:</strong> ${this.formatDateTime(
                              startTime
                            )}
                            <br>
                            <small class="text-warning">
                                <i class="fas fa-clock"></i> 
                                Đã xử lý: <span class="elapsed-time" data-start-time="${startTime}">Đang tính...</span>
                            </small>
                        </div>
                    </div>
                </div>
                <button class="btn-step btn-complete" data-action="complete" data-step-id="${
                  step.buoc_id
                }">
                    <i class="fas fa-check"></i> Hoàn thành
                </button>
                <div class="step-note">
                    <small class="text-success">
                        <i class="fas fa-clock"></i> 
                        Đang xử lý... Nhấn "Hoàn thành" khi đã xong
                    </small>
                </div>
            `;
    } else if (status === "HOAN_THANH") {
      // Get employee and time info for completed step
      const employeeInfo = this.getStepEmployeeInfo(step.buoc_id);
      const startTime = this.getStepStartTime(step.buoc_id);
      const endTime = this.getStepEndTime(step.buoc_id);
      const duration = this.calculateDuration(startTime, endTime);

      actions += `
                <div class="step-completed-info mb-3">
                    <div class="d-flex align-items-center mb-2">
                        <i class="fas fa-user-check text-success me-2"></i>
                        <div>
                            <strong>Người thực hiện:</strong> ${
                              employeeInfo.name || "Không xác định"
                            }
                            <br>
                            <small class="text-muted">${
                              employeeInfo.role || ""
                            }</small>
                        </div>
                    </div>                    
                </div>
                <div class="step-completed">
                    <i class="fas fa-check-circle text-success"></i>
                    <span class="text-success">Đã hoàn thành</span>
                </div>
            `;
    } else if (status === "CHUA_BAT_DAU" && !isCurrent && !isFirstStep) {
      actions += `
                <div class="step-pending">
                    <i class="fas fa-clock text-muted"></i>
                    <span class="text-muted">Chờ các bước trước hoàn thành</span>
                </div>
            `;
    }

    return actions;
  }

  addStepActionListeners(stepElement, step, status) {
    const startBtn = stepElement.querySelector('[data-action="start"]');
    const completeBtn = stepElement.querySelector('[data-action="complete"]');

    startBtn?.addEventListener("click", () => {
      console.log("Start button clicked for step:", step);
      if (step && step.buoc_id) {
        this.startStep(step);
      } else {
        console.error("Step is invalid:", step);
        this.showError("Lỗi: Thông tin bước xử lý không hợp lệ");
      }
    });

    completeBtn?.addEventListener("click", () => {
      console.log("Complete button clicked for step:", step);
      if (step && step.buoc_id) {
        this.completeStep(step);
      } else {
        console.error("Step is invalid:", step);
        this.showError("Lỗi: Thông tin bước xử lý không hợp lệ");
      }
    });
  }

  async startStep(step) {
    try {
      console.log("Starting step:", step);
      console.log("Step buoc_id:", step.buoc_id);
      console.log("Current step:", this.currentStep);
      console.log("Current step ID:", this.currentStepId);

      // Show employee selection modal
      await this.showEmployeeSelection(step, "start");
    } catch (error) {
      console.error("Error starting step:", error);
      this.showError("Không thể bắt đầu bước xử lý");
    }
  }

  async completeStep(step) {
    try {
      console.log("Completing step:", step);

      // Get current employee for this step
      const employeeInfo = this.getStepEmployeeInfo(step.buoc_id);

      if (!employeeInfo.name || employeeInfo.name === "Đang tải...") {
        this.showError("Không thể xác định người xử lý cho bước này");
        return;
      }

      // Confirm completion
      const confirmed = confirm(
        `Xác nhận hoàn thành bước "${step.ten_buoc}"?\n\n` +
          `Người xử lý: ${employeeInfo.name}\n` +
          `Vai trò: ${employeeInfo.role}`
      );

      if (!confirmed) return;

      // Prepare data for API call
      const data = {
        orderId: this.orderId,
        stepId: step.buoc_id,
        employeeId: this.stepStatusData[step.buoc_id]?.employeeId || 0,
        action: "complete",
      };

      // Call API to complete step
      const response = await fetch("/LichSuThucHien/UpdateStepStatus", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector(
            'input[name="__RequestVerificationToken"]'
          )?.value,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error("Failed to complete step");

      const result = await response.json();
      console.log("Step completed:", result);

      // Làm mới timeline để hiển thị trạng thái đã cập nhật
      await this.loadOrderStatus();
      this.renderTimeline();

      // Kiểm tra và cập nhật trạng thái đơn hàng sau khi hoàn thành bước
      await this.checkOrderStatus();

      // Hiển thị thông báo thành công
      this.showSuccess(
        `Bước "${step.ten_buoc}" đã được hoàn thành bởi ${employeeInfo.name}`
      );
    } catch (error) {
      console.error("Error completing step:", error);
      this.showError("Không thể hoàn thành bước xử lý");
    }
  }

  async checkOrderStatus() {
    try {
      console.log("Checking order status for order:", this.orderId);

      const response = await fetch("/LichSuThucHien/CheckOrderStatus", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector(
            'input[name="__RequestVerificationToken"]'
          )?.value,
        },
        body: JSON.stringify({ orderId: this.orderId }),
      });

      if (!response.ok) throw new Error("Failed to check order status");

      const result = await response.json();
      console.log("Order status check result:", result);

      if (result.success) {
        // Làm mới trang để hiển thị trạng thái đơn hàng đã cập nhật
        await this.loadOrderStatus();
        this.renderTimeline();
      }
    } catch (error) {
      console.error("Error checking order status:", error);
      // Không hiển thị lỗi cho người dùng vì đây là thao tác nền
    }
  }

  async showEmployeeSelection(step, action) {
    try {
      console.log(
        `Loading employees for step ${step.buoc_id} (${step.ten_buoc})`
      );

      // Load available employees for this step
      const url = `/PhanCongBuocXuLy/GetNhanVienChoBuoc/${step.buoc_id}`;
      console.log(`Fetching employees from URL: ${url}`);
      console.log(`Step buoc_id being sent: ${step.buoc_id}`);

      const response = await fetch(url);
      if (!response.ok) throw new Error("Failed to load employees");

      const employees = await response.json();
      console.log("Available employees:", employees);

      // Show modal with employee selection
      this.renderEmployeeSelection(employees, step, action);

      // Show modal
      const modal = new bootstrap.Modal(
        document.getElementById("employeeModal")
      );
      modal.show();
    } catch (error) {
      console.error("Error loading employees:", error);
      this.showError("Không thể tải danh sách nhân viên");
    }
  }

  renderEmployeeSelection(employees, step, action) {
    const container = document.getElementById("employeeSelectionContent");
    if (!container) return;

    const actionText = action === "start" ? "bắt đầu" : "hoàn thành";
    const actionIcon = action === "start" ? "fa-play" : "fa-check";

    container.innerHTML = `
            <div class="mb-4">
                <div class="step-info-header">
                    <h6><i class="fas fa-tasks"></i> ${step.ten_buoc}</h6>
                    <p class="text-muted mb-2">${
                      step.mo_ta || "Không có mô tả"
                    }</p>
                    <div class="alert alert-info">
                        <i class="fas ${actionIcon}"></i>
                        <strong>Chọn nhân viên để ${actionText} bước xử lý này</strong>
                    </div>
                </div>
            </div>
            <div class="employee-selection">
                <h6><i class="fas fa-users"></i> Danh sách nhân viên phù hợp với bước này</h6>
                <div class="employee-list">
                    ${
                      employees && employees.length > 0
                        ? employees
                            .map(
                              (emp) => `
                        <div class="employee-card" data-employee-id="${
                          emp.nv_id
                        }">
                            <div class="employee-avatar">
                                <i class="fas fa-user"></i>
                            </div>
                            <div class="employee-info">
                                <div class="employee-name">${
                                  emp.ho_ten || "N/A"
                                }</div>
                                <div class="employee-role">${
                                  emp.loai_nv || "N/A"
                                }</div>
                                <div class="employee-status">
                                    <span class="badge bg-success">Có sẵn</span>
                                </div>
                            </div>
                            <div class="employee-select">
                                <i class="fas fa-check-circle"></i>
                            </div>
                        </div>
                    `
                            )
                            .join("")
                        : `
                        <div class="no-employees-message">
                            <div class="alert alert-warning">
                                <div class="text-center">
                                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                                    <h6>Không có nhân viên phù hợp cho bước này</h6>
                                    <p class="mb-0">
                                        Bước "${step.ten_buoc}" chưa có nhân viên thuộc loại phù hợp hoặc chưa có dữ liệu phân công. 
                                        Vui lòng liên hệ quản trị viên để thiết lập phân công nhân viên cho bước này.
                                    </p>
                                </div>
                            </div>
                        </div>
                    `
                    }
                </div>
            </div>
        `;

    // Store current step and action for confirmation
    this.currentStep = step;
    this.currentAction = action;
    this.selectedEmployee = null;
  }

  selectEmployee(employeeCard) {
    // Remove previous selection
    document.querySelectorAll(".employee-card").forEach((card) => {
      card.classList.remove("selected");
    });

    // Select current employee
    employeeCard.classList.add("selected");
    this.selectedEmployee = {
      id: employeeCard.dataset.employeeId,
      name: employeeCard.querySelector(".employee-name").textContent,
      role: employeeCard.querySelector(".employee-role").textContent,
    };

    console.log("Selected employee:", this.selectedEmployee);
  }

  async confirmEmployeeSelection() {
    if (!this.selectedEmployee) {
      this.showError("Vui lòng chọn nhân viên");
      return;
    }

    try {
      console.log("Confirming employee selection:", this.selectedEmployee);
      console.log("Current step:", this.currentStep);
      console.log("Current action:", this.currentAction);

      // Prepare data for API call
      const data = {
        orderId: this.orderId,
        stepId: this.currentStep.buoc_id,
        employeeId: parseInt(this.selectedEmployee.id),
        action: this.currentAction,
      };

      // Call API to update step status
      const response = await fetch("/LichSuThucHien/UpdateStepStatus", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: document.querySelector(
            'input[name="__RequestVerificationToken"]'
          )?.value,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error("Failed to update step status");

      const result = await response.json();
      console.log("Step status updated:", result);

      // Close modal
      const modal = bootstrap.Modal.getInstance(
        document.getElementById("employeeModal")
      );
      modal.hide();

      // Refresh timeline to show updated status
      await this.loadOrderStatus();
      this.renderTimeline();

      // Show success message
      this.showSuccess(
        `Bước "${this.currentStep.ten_buoc}" đã được ${
          this.currentAction === "start" ? "bắt đầu" : "hoàn thành"
        } bởi ${this.selectedEmployee.name}`
      );
    } catch (error) {
      console.error("Error confirming employee selection:", error);
      this.showError("Không thể cập nhật trạng thái bước xử lý");
    }
  }

  showSuccess(message) {
    // Create and show success toast
    const toast = document.createElement("div");
    toast.className = "toast align-items-center text-white bg-success border-0";
    toast.setAttribute("role", "alert");
    toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-check-circle me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

    // Append to toast container
    const toastContainer = document.querySelector(".toast-container");
    if (toastContainer) {
      toastContainer.appendChild(toast);
    } else {
      document.body.appendChild(toast);
    }

    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    // Remove toast after it's hidden
    toast.addEventListener("hidden.bs.toast", () => {
      if (toastContainer) {
        toastContainer.removeChild(toast);
      } else {
        document.body.removeChild(toast);
      }
    });
  }

  showError(message) {
    // Create and show error toast
    const toast = document.createElement("div");
    toast.className = "toast align-items-center text-white bg-danger border-0";
    toast.setAttribute("role", "alert");
    toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-exclamation-circle me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

    // Append to toast container
    const toastContainer = document.querySelector(".toast-container");
    if (toastContainer) {
      toastContainer.appendChild(toast);
    } else {
      document.body.appendChild(toast);
    }

    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    // Remove toast after it's hidden
    toast.addEventListener("hidden.bs.toast", () => {
      if (toastContainer) {
        toastContainer.removeChild(toast);
      } else {
        document.body.removeChild(toast);
      }
    });
  }
}

// Initialize when DOM is loaded
let orderWorkflowInstance = null;

document.addEventListener("DOMContentLoaded", () => {
  orderWorkflowInstance = new OrderWorkflow();
});

// Cleanup when page unloads
window.addEventListener("beforeunload", () => {
  if (orderWorkflowInstance && orderWorkflowInstance.timerInterval) {
    clearInterval(orderWorkflowInstance.timerInterval);
  }
});
