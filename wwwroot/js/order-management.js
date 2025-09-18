// Order Management JavaScript
// Global variables
let selectedMenuItems = {};
let currentStep = "create";

// Initialize page
document.addEventListener("DOMContentLoaded", function () {
  initializeEventListeners();
  updateTotalAmount();
});

// Event listeners
function initializeEventListeners() {
  // Step tabs
  document.querySelectorAll(".step-tab").forEach((tab) => {
    tab.addEventListener("click", function () {
      switchStep(this.dataset.step);
    });
  });

  // Order type change
  document.getElementById("orderType").addEventListener("change", function () {
    toggleTableSelection(this.value === "false");
  });

  // Table selection
  document.getElementById("tableId").addEventListener("change", function () {
    validateTableCapacity();
  });

  // Customer count
  document
    .getElementById("customerCount")
    .addEventListener("input", function () {
      validateTableCapacity();
    });

  // Menu item selection
  document.querySelectorAll(".menu-item").forEach((item) => {
    item.addEventListener("click", function () {
      const monId = parseInt(this.dataset.monId);
      const quantityInput = document.getElementById(`qty-${monId}`);
      const currentQty = parseInt(quantityInput.value);

      if (currentQty === 0) {
        quantityInput.value = 1;
        this.classList.add("selected");
      } else {
        quantityInput.value = 0;
        this.classList.remove("selected");
      }

      updateMenuSelection(monId, parseInt(quantityInput.value));
      updateTotalAmount();
    });
  });

  // Form submission
  document
    .getElementById("createOrderForm")
    .addEventListener("submit", function (e) {
      e.preventDefault();
      createOrder();
    });
}

// Step management
function switchStep(step) {
  // Update tabs
  document.querySelectorAll(".step-tab").forEach((tab) => {
    tab.classList.remove("active");
  });
  document.querySelector(`[data-step="${step}"]`).classList.add("active");

  // Update panels
  document.querySelectorAll(".step-panel").forEach((panel) => {
    panel.classList.remove("active");
  });
  document.getElementById(`${step}-panel`).classList.add("active");

  currentStep = step;
}

// Table management
function toggleTableSelection(show) {
  const tableGroup = document.getElementById("tableGroup");
  const tableSelect = document.getElementById("tableId");
  const customerCountInput = document.getElementById("customerCount");

  if (show) {
    tableGroup.style.display = "block";
    tableSelect.required = true;
    customerCountInput.required = true;
  } else {
    tableGroup.style.display = "none";
    tableSelect.required = false;
    customerCountInput.required = false;
    tableSelect.value = "";
    customerCountInput.value = "";
  }
}

function validateTableCapacity() {
  const tableSelect = document.getElementById("tableId");
  const customerCount = document.getElementById("customerCount").value;
  const selectedTable = tableSelect.options[tableSelect.selectedIndex];

  if (selectedTable && selectedTable.value && customerCount) {
    const capacity = parseInt(selectedTable.dataset.capacity);
    const customers = parseInt(customerCount);

    if (customers > capacity) {
      alert(
        `Bàn này chỉ có thể phục vụ tối đa ${capacity} khách. Vui lòng chọn bàn khác hoặc giảm số khách.`
      );
      tableSelect.value = "";
    }
  }
}

// Menu management
function increaseQuantity(monId) {
  const input = document.getElementById(`qty-${monId}`);
  const currentValue = parseInt(input.value);
  if (currentValue < 10) {
    input.value = currentValue + 1;
    updateMenuSelection(monId, currentValue + 1);
    updateTotalAmount();
  }
}

function decreaseQuantity(monId) {
  const input = document.getElementById(`qty-${monId}`);
  const currentValue = parseInt(input.value);
  if (currentValue > 0) {
    input.value = currentValue - 1;
    updateMenuSelection(monId, currentValue - 1);
    updateTotalAmount();
  }
}

function updateMenuSelection(monId, quantity) {
  const menuItem = document.querySelector(`[data-mon-id="${monId}"]`);

  if (quantity > 0) {
    selectedMenuItems[monId] = quantity;
    menuItem.classList.add("selected");
  } else {
    delete selectedMenuItems[monId];
    menuItem.classList.remove("selected");
  }
}

function updateTotalAmount() {
  let total = 0;

  Object.keys(selectedMenuItems).forEach((monId) => {
    const menuItem = document.querySelector(`[data-mon-id="${monId}"]`);
    const price = parseFloat(menuItem.dataset.price);
    const quantity = selectedMenuItems[monId];
    total += price * quantity;
  });

  document.getElementById("totalAmount").textContent = total.toLocaleString();
}

// Order creation
function createOrder() {
  const form = document.getElementById("createOrderForm");
  const formData = new FormData(form);

  // Debug: Log form data
  console.log("=== DEBUG FORM DATA ===");
  for (let [key, value] of formData.entries()) {
    console.log(`${key}: ${value}`);
  }

  // Debug: Log individual form elements
  console.log("=== DEBUG FORM ELEMENTS ===");
  console.log("orderType value:", document.getElementById("orderType").value);
  console.log("tableId value:", document.getElementById("tableId").value);
  console.log(
    "customerCount value:",
    document.getElementById("customerCount").value
  );
  console.log(
    "customerName value:",
    document.getElementById("customerName").value
  );
  console.log(
    "customerPhone value:",
    document.getElementById("customerPhone").value
  );

  // Prepare order items
  const orderItems = [];
  Object.keys(selectedMenuItems).forEach((monId) => {
    orderItems.push({
      MonId: parseInt(monId),
      SoLuong: selectedMenuItems[monId],
    });
  });

  console.log("Selected menu items:", selectedMenuItems);
  console.log("Order items:", orderItems);

  if (orderItems.length === 0) {
    alert("Vui lòng chọn ít nhất một món ăn");
    return;
  }

  // Get form values
  const isTakeaway = formData.get("LaMangVe") === "true";
  const tableId = formData.get("BanId");
  const customerCount = formData.get("SoKhach");

  console.log("Form values:");
  console.log("- isTakeaway:", isTakeaway);
  console.log("- tableId:", tableId);
  console.log("- customerCount:", customerCount);

  // Validate dine-in orders
  if (!isTakeaway) {
    if (!tableId || tableId === "") {
      alert("Đơn hàng ăn tại chỗ phải chọn bàn");
      return;
    }
    if (
      !customerCount ||
      customerCount === "" ||
      parseInt(customerCount) <= 0
    ) {
      alert("Đơn hàng ăn tại chỗ phải có số khách");
      return;
    }
  }

  // Validate takeaway orders
  if (isTakeaway && tableId && tableId !== "") {
    alert("Đơn hàng mang về không được chọn bàn");
    return;
  }

  // Prepare data
  const orderData = {
    LaMangVe: isTakeaway,
    BanId: tableId && tableId !== "" ? parseInt(tableId) : null,
    SoKhach:
      customerCount && customerCount !== "" ? parseInt(customerCount) : null,
    KhachHangTen: formData.get("KhachHangTen"),
    KhachHangSdt: formData.get("KhachHangSdt"),
    OrderItems: orderItems,
  };

  console.log("Final order data to send:", orderData);

  // Show loading
  const submitBtn = document.querySelector(
    '#createOrderForm button[type="submit"]'
  );
  const originalText = submitBtn.innerHTML;
  submitBtn.innerHTML =
    '<i class="fas fa-spinner fa-spin"></i> Đang tạo đơn hàng...';
  submitBtn.disabled = true;

  // Send request using JSON
  console.log("Sending request to /Home/CreateOrder");

  fetch("/Home/CreateOrder", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value,
    },
    body: JSON.stringify(orderData),
  })
    .then((response) => {
      console.log("Response status:", response.status);
      return response.json();
    })
    .then((data) => {
      console.log("Response data:", data);
      if (data.success) {
        alert(`Đơn hàng đã được tạo thành công! Mã đơn hàng: ${data.orderId}`);
        resetCreateForm();
      } else {
        alert(`Lỗi: ${data.message}`);
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      alert("Có lỗi xảy ra khi tạo đơn hàng");
    })
    .finally(() => {
      submitBtn.innerHTML = originalText;
      submitBtn.disabled = false;
    });
}

function resetCreateForm() {
  document.getElementById("createOrderForm").reset();
  selectedMenuItems = {};
  document.querySelectorAll(".menu-item").forEach((item) => {
    item.classList.remove("selected");
  });
  document.querySelectorAll(".quantity-input").forEach((input) => {
    input.value = 0;
  });
  updateTotalAmount();
}
