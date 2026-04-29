<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <strong class="text-2xl">Danh sách người dùng</strong>
        <div class="flex gap-3">
            <Button
                label="Thêm mới"
                icon="fa-solid fa-plus"
                class="bg-info-700"
                @click="newAndSaveItems()"
            />
        </div>
    </div>

    <div class="grid mt-3 card p-2">
        <div class="col-12 h-screen bg-white">
            <TabView>
                <TabPanel header="APSP">
                    <DataTable
                        :value="dataUsersApsp"
                        :paginator="true"
                        stripedRows
                        header="surface-200"
                        dataKey="id"
                        showGridlines
                        :rows="dataTable.sizeApsp"
                        :page="dataTable.pageApsp"
                        :totalRecords="dataTable.totalApsp"
                        @page="onPageChange($event)"
                        :rowsPerPageOptions="[10, 20, 30]"
                        lazy
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} người dùng"
                    >
                        <template #header>
                            <div class="flex justify-content-between m-0 mb-3">
                                <IconField iconPosition="left">
                                    <InputText placeholder="Nhập từ khóa tìm kiếm" />
                                    <InputIcon>
                                        <i class="pi pi-search" />
                                    </InputIcon>
                                </IconField>
                                <Button
                                    type="button"
                                    icon="pi pi-filter-slash"
                                    label="Xóa bộ lọc"
                                    outlined
                                    @click="clearFilter()"
                                />
                            </div>
                        </template>
                        <Column header="#" :style="{ width: '5%' }">
                            <template #body="slotProps">
                                {{ slotProps.index + 1 }}
                            </template>
                        </Column>
                        <Column field="userName" header="Tên người dùng"></Column>
                        <Column field="email" header="Email"></Column>
                        <Column field="fullName" header="Họ và tên"></Column>
                        <Column field="userType" header="Vai trò"></Column>
                        <Column field="action" header="Hành động" class="w-10rem">
                            <template #body="slotProps">
                                <div class="flex justify-content-center gap-3">
                                    <Button
                                        @click="newAndSaveItems(slotProps.data)"
                                        icon="pi pi-pencil"
                                        text
                                    />
                                    <Button
                                        @click="deleteUser(slotProps.data)"
                                        severity="danger"
                                        icon="pi pi-trash"
                                        text
                                    />
                                </div>
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>
                <TabPanel header="Khách hàng">
                    <DataTable
                        :value="dataUsersNpp"
                        :paginator="true"
                        stripedRows
                        header="surface-200"
                        dataKey="id"
                        showGridlines
                        :rows="dataTable.sizeNpp"
                        :page="dataTable.pageNpp"
                        :totalRecords="dataTable.totalNpp"
                        @page="onPageChange($event, 'NPP')"
                        :rowsPerPageOptions="[10, 20, 30]"
                        lazy
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} người dùng"
                    >
                        <template #header>
                            <div class="flex justify-content-between m-0 mb-3">
                                <IconField iconPosition="left">
                                    <InputText placeholder="Nhập từ khóa tìm kiếm" />
                                    <InputIcon>
                                        <i class="pi pi-search" />
                                    </InputIcon>
                                </IconField>
                                <Button
                                    type="button"
                                    icon="pi pi-filter-slash"
                                    label="Xóa bộ lọc"
                                    outlined
                                    @click="clearFilter()"
                                />
                            </div>
                        </template>
                        <Column header="#" :style="{ width: '5%' }">
                            <template #body="slotProps">
                                {{ slotProps.index + 1 }}
                            </template>
                        </Column>
                        <Column field="userName" header="Tên người dùng"></Column>
                        <Column field="email" header="Email"></Column>
                        <Column field="fullName" header="Họ và tên"></Column>
                        <Column field="userType" header="Vai trò"></Column>
                        <Column field="action" header="Hành động" class="w-10rem">
                            <template #body="slotProps">
                                <div class="flex justify-content-center gap-3">
                                    <Button
                                        @click="newAndSaveItems(slotProps.data)"
                                        icon="pi pi-pencil"
                                        text
                                    />
                                    <Button
                                        @click="deleteUser(slotProps.data)"
                                        severity="danger"
                                        icon="pi pi-trash"
                                        text
                                    />
                                </div>
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>
            </TabView>
        </div>
    </div>

    <Dialog
        v-model:visible="dialogOnOffDelete"
        modal
        position="top"
        :draggable="false"
        header="Xác nhận"
        :style="{ width: '35%' }"
        class="p-fluid"
    >
        <div
            class="flex flex-column align-items-center w-full gap-3 border-bottom-1 surface-border pb-0"
        >
            <i class="pi pi-info-circle text-6xl text-red-500"></i>
            <p>Bạn có chắc chắn xoá người dùng {{ dataEditDelete.userName }} không?</p>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    label="Huỷ"
                    outlined
                    severity="secondary"
                    @click="dialogOnOffDelete = false"
                />
                <Button label="Xoá" @click="confirmDelete(true)" severity="danger" />
            </div>
        </template>
    </Dialog>

    <Dialog
        v-model:visible="dialogItem"
        modal
        :draggable="false"
        :header="payload.id ? 'Chỉnh sửa người dùng' : 'Thêm mới người dùng'"
        :style="{ width: '50%' }"
        class="p-fluid"
    >
        <div class="card">
            <div class="grid">
                <div class="col-6">
                    <label for="email" class="inline-block mb-2"
                        >Địa chỉ Email <span class="text-red-500">*</span></label
                    >
                    <InputText
                        v-model="payload.email"
                        class="input-text"
                        :invalid="submited && payload.email == ''"
                        type="email"
                        :disabled="payload.id"
                    />
                    <small class="text-red-500" v-if="submited && payload.email == ''"
                        >Vui lòng nhập đúng địa chỉ email<br
                    /></small>
                    <small class="text-red-500" v-if="submited && !validateEmail()"
                        >Địa chỉ email không hợp lệ</small
                    >
                </div>
                <div class="col-6">
                    <label for="userName" class="inline-block mb-2"
                        >Tên tài khoản <span class="text-red-500">*</span></label
                    >
                    <InputText
                        v-model="payload.userName"
                        class="input-text"
                        :invalid="submited && payload.userName == ''"
                        :disabled="payload.id"
                    />
                    <small class="text-red-500" v-if="submited && payload.userName == ''"
                        >Vui lòng nhập tên tài khoản<br
                    /></small>
                    <small class="text-red-500" v-if="submited && !validateUserName()"
                        >Tên tài khoản không được cách nhau bằng dấu cách</small
                    >
                </div>
                <div class="col-6">
                    <label for="fullName" class="inline-block mb-2"
                        >Họ và Tên <span class="text-red-500">*</span></label
                    >
                    <InputText
                        v-model="payload.fullName"
                        class="input-text"
                        :invalid="submited && payload.fullName == ''"
                    />
                    <small class="text-red-500" v-if="submited && payload.fullName == ''"
                        >Vui lòng nhập họ và tên</small
                    >
                </div>
                <div class="col-6">
                    <label for="phone" class="inline-block mb-2"
                        >Số điện thoại <span class="text-red-500">*</span></label
                    >
                    <InputText
                        id="phone"
                        v-model="payload.phone"
                        :invalid="submited && payload.phone == ''"
                    />
                    <small class="text-red-500" v-if="submited && payload.phone == ''"
                        >Vui lòng nhập số điện thoại<br
                    /></small>
                    <small
                        class="text-red-500"
                        v-if="submited && payload.phone?.length != 10"
                        >Số điện thoại phải có 10 chữ số</small
                    >
                </div>
                <div class="col-6">
                    <label for="password" class="inline-block mb-2"
                        >Mật khẩu <span class="text-red-500">*</span></label
                    >
                    <Password
                        toggleMask
                        v-model="payload.password"
                        class="input-text"
                        :invalid="submited && payload.password == ''"
                    >
                        <template #footer>
                            <Divider />
                            <p class="mt-2">Yêu cầu</p>
                            <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
                                <li>Ít nhất một chữ thường</li>
                                <li>Ít nhất một chữ hoa</li>
                                <li>Ít nhất một số</li>
                                <li>Ít nhất ký tự</li>
                                <li>Ít nhất 8 ký tự</li>
                            </ul>
                        </template>
                    </Password>
                    <small class="text-red-500" v-if="submited && payload.password == ''"
                        >Vui lòng nhập mật khẩu</small
                    >
                </div>
                <div class="col-6">
                    <label for="confirmPassword" class="inline-block mb-2"
                        >Xác nhận mật khẩu <span class="text-red-500">*</span></label
                    >
                    <Password
                        toggleMask
                        v-model="payload.confirmPassword"
                        class="input-text"
                        :invalid="submited && payload.confirmPassword == ''"
                    >
                        <template #footer>
                            <Divider />
                            <p class="mt-2">Yêu cầu</p>
                            <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
                                <li>Ít nhất một chữ thường</li>
                                <li>Ít nhất một chữ hoa</li>
                                <li>Ít nhất một số</li>
                                <li>Ít nhất ký tự</li>
                                <li>Ít nhất 8 ký tự</li>
                            </ul>
                        </template>
                    </Password>
                    <small
                        class="text-red-500"
                        v-if="submited && payload.confirmPassword == ''"
                        >Vui lòng xác nhận mật khẩu<br />
                    </small>
                    <small class="text-red-500" v-if="!validatePassword2()"
                        >Không khớp mật khẩu</small
                    >
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex justify-end gap-2">
                <Button
                    type="button"
                    label="Huỷ"
                    severity="secondary"
                    @click="dialogItem = false"
                    class="button button-cancel"
                />
                <Button
                    type="button"
                    label="Lưu"
                    @click="saveUser()"
                    class="button button-save"
                />
            </div>
        </template>
    </Dialog>

    <Loading v-if="loading" />
</template>

<script setup>
import { ref, onBeforeMount, computed } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
const loading = ref(false);
const { toast, FunctionGlobal } = useGlobal();
const dataUsersApsp = ref([]);
const dataUsersNpp = ref([]);
const dataTable = ref({
    pageApsp: 0,
    sizeApsp: 10,
    pageNpp: 0,
    sizeNpp: 10,
    totalApsp: 0,
    totalNpp: 0,
});
const dialogItem = ref(false);
const payload = ref();
const dialogOnOffDelete = ref(false);
const dataEditDelete = ref({});
onBeforeMount(() => {
    fetchAllUsersApsp();
    fetchAllUsersNpp();
});

const InitData = () => {
    payload.value = {
        id: 0,
        email: "",
        fullName: "",
        phone: "",
        password: "",
        confirmPassword: "",
        userType: "APSP",
        userName: "",
        status: "A",
    };
};

InitData();

const newAndSaveItems = async (data = null) => {
    if (data?.id) {
        payload.value = data;
    } else {
        InitData();
    }
    dialogItem.value = true;
};

const fetchAllUsersApsp = async () => {
    try {
        loading.value = true;
        const query = `Account/getall?limit=${dataTable.value.sizeApsp}&skip=${dataTable.value.pageApsp}&userType=APSP`;
        const {
            data: { item, total },
        } = await API.get(query);
        dataUsersApsp.value = item;
        dataTable.value.totalApsp = total;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

const fetchAllUsersNpp = async () => {
    try {
        loading.value = true;
        const query = `Account/getall?limit=${dataTable.value.sizeNpp}&skip=${dataTable.value.pageNpp}&userType=NPP`;
        const {
            data: { item, total },
        } = await API.get(query);
        dataUsersNpp.value = item;
        dataTable.value.totalNpp = total;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

const deleteUser = (data) => {
    dataEditDelete.value = data;
    dialogOnOffDelete.value = true;
};

const confirmDelete = async (confirm) => {
    if (confirm) {
        try {
            loading.value = true;
            await API.delete(`Account/${dataEditDelete.value.id}`);
            fetchAllUsersApsp();
            FunctionGlobal.$notify("S", "Người dùng đã được xoá thành công", toast);
        } catch (error) {
            FunctionGlobal.$notify("E", error, toast);
        } finally {
            loading.value = false;
        }
    }
    dialogOnOffDelete.value = false;
};

const submited = ref(false);
const validate = () => {
    const { email, userName, fullName, phone, password, confirmPassword } = payload.value;
    return Boolean(
        email?.trim() &&
            userName?.trim() &&
            fullName?.trim() &&
            phone?.trim() &&
            password?.trim() &&
            confirmPassword?.trim()
    );
};

const validatePassword = () => {
    const { password, confirmPassword } = payload.value;
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*(_|[^\w])).{8,}$/;
    return (
        passwordRegex.test(password) &&
        passwordRegex.test(confirmPassword) &&
        password === confirmPassword
    );
};
const validatePassword2 = () => {
    const { password, confirmPassword } = payload.value;
    return password === confirmPassword;
};
const validateEmail = () => {
    const { email } = payload.value;
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
};
const validateUserName = () => {
    const { userName } = payload.value;
    return !userName.includes(" ");
};

const saveUser = async () => {
    submited.value = true;
    if (!payload.value.id) {
        if (!validate()) {
            FunctionGlobal.$notify("E", "Vui lòng nhập đẩy đủ các trường", toast);
            return;
        }
        if (!validatePassword()) {
            FunctionGlobal.$notify("E", "Mật khẩu chưa thoả mãn yêu cầu", toast);
            return;
        }
        if (!validatePassword2()) {
            FunctionGlobal.$notify("E", "Mật khẩu không khớp", toast);
            return;
        }
    }

    if (!validateEmail()) {
        FunctionGlobal.$notify("E", "Địa chỉ email không hợp lệ", toast);
        return;
    }
    if (!validateUserName()) {
        FunctionGlobal.$notify(
            "E",
            "Tên tài khoản không được cách nhau bằng dấu cách",
            toast
        );
        return;
    }
    try {
        if (payload.value.id) {
            const res = await API.update(`Account/${payload.value.id}`, payload.value);
            FunctionGlobal.$notify("S", "Người dùng đã được chỉnh sửa thành công", toast);
        } else {
            const res = await API.add(`Account/Register`, payload.value);
            FunctionGlobal.$notify("S", "Người dùng đã được thêm thành công", toast);
        }
        InitData();
        fetchAllUsersApsp();
        dialogItem.value = false;
    } catch (error) {
        if (error.response.data.errors) {
            FunctionGlobal.$notify("E", error.response.data.errors, toast);
        } else {
            FunctionGlobal.$notify("E", error, toast);
        }
    } finally {
        loading.value = false;
    }
};

const onPageChange = (event, type = null) => {
    if (type == "NPP") {
        dataTable.value.pageNpp = event.page;
        dataTable.value.sizeNpp = event.rows;
        fetchAllUsersNpp();
    } else {
        dataTable.value.pageApsp = event.page;
        dataTable.value.sizeApsp = event.rows;
        fetchAllUsersApsp();
    }
};
</script>

<style></style>
