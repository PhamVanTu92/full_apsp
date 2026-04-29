<template>
  <div class="grid justify-content-center px-5 py-5">
    <div class="col-9">
      <div class="flex flex-column align-items-center justify-content-center">
        <div
          class="w-full"
          style="
            border-radius: 56px;
            padding: 0.3rem;
            background: linear-gradient(
              180deg,
              var(--primary-color) 10%,
              rgba(33, 150, 243, 0) 30%
            );
          "
        >
          <div class="w-full surface-card py-3 px-5 sm:px-8" style="border-radius: 53px">
            <div class="text-center align-items-center flex flex-column mb-4">
              <img
                src="@/assets/images/logo-removebg-preview.png"
                class="mb-3"
                height="100"
                alt="logo"
              />
              <strong class="text-2xl text-primary mb-3">Saigon Petro OmniChannel</strong>
              <span class="text-600 font-medium"
                ><router-link to="/login"
                  ><b class="text-blue-700">Đăng nhập</b></router-link
                >
                nếu đã có tài khoản</span
              >
            </div>
            <div class="grid card bg-gray-50 mt-3">
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold">Họ</label>
                  <InputText
                    type="text"
                    class="w-full"
                    v-model="userCredentials.first_name"
                  />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold">Tên đệm</label>
                  <InputText
                    type="text"
                    class="w-full"
                    v-model="userCredentials.last_name"
                  />
                </div>
              </div>
              <div class="col-12">
                <div class="field">
                  <label class="block text-900 text-base font-semibold">Email</label>
                  <InputText type="text" class="w-full" v-model="userCredentials.email" />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold"
                    >Tên người dùng</label
                  >
                  <InputText
                    type="text"
                    class="w-full"
                    v-model="userCredentials.username"
                  />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold"
                    >Số điện thoại</label
                  >
                  <InputText
                    type="text"
                    class="w-full"
                    v-model="userCredentials.phone_number"
                  />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold">Mật khẩu</label>
                  <Password class="w-full" v-model="userCredentials.password" />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold"
                    >Xác nhận mật khẩu</label
                  >
                  <InputText
                    type="password"
                    class="w-full"
                    v-model="userCredentials.Repassword"
                  />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold">Giới tính</label>
                  <Dropdown
                    class="w-full"
                    :options="[
                      { code: 1, name: 'Nam' },
                      { code: 0, name: 'Nữ' },
                      { code: 3, name: 'Khác' },
                    ]"
                    optionLabel="name"
                    optionValue="code"
                    v-model="userCredentials.gender"
                  />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label class="block text-900 text-base font-semibold"
                    >Ngày/Tháng/Năm sinh</label
                  >
                  <Calendar
                    class="w-full"
                    v-model="userCredentials.date_of_birth"
                    dateFormat="dd/mm/yy"
                  />
                </div>
              </div>
              <div class="col-12">
                <div class="field">
                  <Button
                    label="Đăng ký tài khoản"
                    @click="handleSubmit"
                    class="p-3 w-full"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <Toast position="bottom-right" />
</template>

<style scoped>
.pi-eye {
  transform: scale(1.6);
  margin-right: 1rem;
}

.pi-eye-slash {
  transform: scale(1.6);
  margin-right: 1rem;
}
:deep(.p-inputtext) {
  width: 100%;
}
</style>
<script setup>
import { ref, onMounted } from "vue";
import { useAuthStore } from "@/Pinia/auth";
import { useRouter } from "vue-router";
import { useGlobal } from "@/services/useGlobal";

const { toast, FunctionGlobal } = useGlobal();
const router = useRouter();
const authStore = useAuthStore();
const userCredentials = ref({
  user_type: "BC",
});

const handleSubmit = async () => {
  const res = await authStore.register(userCredentials.value);
  if (!res.status) {
    FunctionGlobal.$notify("E", res.message, toast);
    return;
  } else {
    FunctionGlobal.$notify("S", "Chào mừng bạn đã đăng ký thành công", toast);
    router.push("/client");
  }
};

const validate = () => {
  let status = true;
  const d = userCredentials.value;
  if (d.email && !validateEmail(d.email)) {
    status = false;
    FunctionGlobal.$notify("E", "Sai định dạng email", toast);
  }

  if (d.password != d.Repassword) {
    status = false;
    FunctionGlobal.$notify("E", "Mật khẩu không khớp nhau", toast);
  }

  if (
    !d.first_name ||
    !d.last_name ||
    !d.gender ||
    !d.password ||
    !d.username ||
    !d.email ||
    !d.phone_number
  ) {
    status = false;
    FunctionGlobal.$notify("E", "Vui lòng nhập đủ thông tin", toast);
  }
  return status;
};
const validateEmail = (email) => {
  return String(email)
    .toLowerCase()
    .match(
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    );
};

const validatePhoneNumber = (phone) => {
  const phoneRegex = /^\d{10}$/;
  return phoneRegex.test(phone);
};
</script>
