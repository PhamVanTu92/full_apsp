<script lang="ts" setup>
import { ref, onBeforeMount, reactive } from "vue";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";

interface RoleClaim {
    privilegesId: number;
    checked: boolean;
    partialChecked: boolean;
}
class RoleClaim {}

interface Role {
    id: number | null;
    name: string | null;
    normalizedName: string | null;
    concurrencyStamp: string | null;
    roleClaims: Array<RoleClaim>;
    privilegeIds: Array<number> | null;
    selectClaims: Object;
}

class Role {
    constructor() {
        this.roleClaims = Array<RoleClaim>();
        this.privilegeIds = Array<number>();
        this.name = null;
    }
    toJSON() {
        for (const key in this.selectClaims) {
            const selectClaim = this.selectClaims[key] as RoleClaim;
            const keyNumber = parseInt(key);
            selectClaim.privilegesId = keyNumber;
            this.roleClaims.push(selectClaim);
            this.privilegeIds.push(keyNumber);
        }
        const { selectClaims, ...role } = this;
        return JSON.parse(JSON.stringify(role));
    }
}

const data = reactive({
    roles: [],
    claims: [],
});

const toast = useToast();
const visible = ref(false);

const payload = ref<Role>(new Role());
const loading = reactive({
    role: false,
});

const selectedRole = ref();

const fetchRoles = (): void => {
    API.get("role/getrole")
        .then((res) => {
            data.roles = res.data;
        })
        .catch((error) => {});
};

const fetchClaims = (): void => {
    API.get("Privileges/getall")
        .then((res) => {
            data.claims = res.data;
        })
        .catch();
};

const onClickSaveRole = (): void => {
    loading.role = true;
    const dataBody = payload.value.toJSON();
    API.add("role/add", dataBody)
        .then((res) => {
            loading.role = false;
            data.roles.push(res.data);
            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Thêm mới thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            loading.role = false;
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                life: 3000,
            });
        });
};

onBeforeMount(() => {
    // getAllRole();
    fetchRoles();
    fetchClaims();
});
</script>
<template>
    <div>
        <h3 class="font-bold mb-4" style="line-height: 33px">
            Cập nhật quyền người dùng
        </h3>
        <div class="card">
            <div class="flex gap-5 align-items-center">
                <label for="">Vai trò</label>
                <div class="flex">
                    <Dropdown
                        v-model="selectedRole"
                        :options="data.roles"
                        optionLabel="name"
                        placeholder="Chọn vai trò"
                        class="w-full md:w-14rem"
                    />
                    <!-- @change="getByIdRole(idRole)" -->
                    <Button icon="pi pi-pencil" v-tooltip="'Sửa vai trò'" text/>
                    <Button
                        icon="pi pi-clone"
                        v-tooltip="'Lưu thành vai trò mới'"
                        text
                    />
                    <Button
                        icon="pi pi-plus"
                        v-tooltip="'Thêm vai trò'"
                        text
                        @click="visible = true"
                    />
                </div>
            </div>
        </div>
    </div>
    <Dialog
        v-model:visible="visible"
        modal
        header="Thêm mới vai trò"
        :style="{ width: '95rem' }"
    >
        <div>
            <InputText
                v-model="payload.name"
                class="mb-3"
                placeholder="Nhập tên vai trò"
            ></InputText>
        </div>
        <ClaimsComponent
            v-model="payload.selectClaims"
            :claims="claims"
        ></ClaimsComponent>
        <!-- {{ payload.selectClaims }} -->
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button
                    label="Bỏ qua"
                    severity="secondary"
                    @click="visible = false"
                />
                <Button
                    :loading="loading.role"
                    label="Lưu"
                    @click="onClickSaveRole"
                />
            </div>
        </template>
    </Dialog>
</template>
