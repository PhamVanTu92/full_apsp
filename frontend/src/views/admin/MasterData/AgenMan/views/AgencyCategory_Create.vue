<!-- eslint-disable vue/no-unused-vars -->
<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <div class="text-2xl font-semibold uppercase">Tạo mới nhà cung cấp</div>
    </div>
    <div class="content card grid m-0 p-3">
        <div class="md:col-3 col-12">
            <div class="card border-noround">
                <div class="flex justify-content-center">
                    <Image imageClass="_InputText__" src="https://tse4.mm.bing.net/th?id=OIP.wRtvON_8JKRQghdROw5QvQHaHa&pid=Api&P=0&h=180" alt="Image" width="150" height="150" />
                </div>
                <div class="flex justify-content-end">
                    <Button icon="fa-solid fa-pen-to-square" outlined/>
                </div>
            </div>
        </div>
        <div class="md:col-9 col-12">
            <h4 class="font-semibold border-bottom-1 border-300 text-700 pb-2 mb-4">Thông tin chung</h4>
            <div class="formgrid grid">
                <div class="field col-8">
                    <label for="firstname2">Tên nhà cung cấp</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Mã số thuế</label>
                    <InputText id="lastname2" class="w-full" type="text" />
                </div>
                <div class="field col-6">
                    <label for="lastname2">Email</label>
                    <InputText id="lastname2" class="w-full" type="email" />
                </div>
                <div class="field col-6">
                    <label for="lastname2">Số điện thoại</label>
                    <InputText id="lastname2" class="w-full" type="text" />
                </div>
                <div class="field col-12">
                    <label for="lastname2">Địa chỉ</label>
                    <InputText id="lastname2" class="w-full" type="text" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Tỉnh/Thành phố</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Quận/Huyện</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Xã/Phường</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-12">
                    <label for="lastname2">Website</label>
                    <InputText class="w-full" />
                </div>
                <div class="field col-12">
                    <label for="lastname2">Mô tả</label>
                    <Textarea class="w-full" rows="5" />
                </div>
                <div class="field col-12 flex justify-content-end mt-2">
                    <Button icon="pi pi-save" label="Lưu thông tin"/>
                </div>
            </div>
            <h4 class="font-semibold border-bottom-1 border-300 text-700 pb-2 mb-4">Thông tin liên hệ</h4>
            <DataTable :value="lienHeData">
                <Column header="Tên người liên hệ">
                    <template #body="sp">
                        <span v-if="!sp.data.notSave">{{ sp.data.tenNguoiLienHe }}</span>
                        <InputText v-else v-model="sp.data.tenNguoiLienHe" style="width: 12rem" />
                    </template>
                </Column>
                <Column header="Chức vụ">
                    <template #body="sp">
                        <span v-if="!sp.data.notSave">{{ sp.data.chucVu }}</span>
                        <InputText v-else v-model="sp.data.chucVu" style="width: 10rem" />
                    </template>
                </Column>
                <Column header="Số điện thoại 1">
                    <template #body="sp">
                        <span v-if="!sp.data.notSave">{{ sp.data.soDienThoai1 }}</span>
                        <InputText v-else v-model="sp.data.soDienThoai1" style="width: 10rem" />
                    </template>
                </Column>
                <Column header="Email">
                    <template #body="sp">
                        <span v-if="!sp.data.notSave">{{ sp.data.email }}</span>
                        <InputText v-else v-model="sp.data.email" />
                    </template>
                </Column>
                // eslint-disable-next-line vue/no-unused-vars
                <template #footer="sp">
                    <Button @click="appendContact" icon="pi pi-plus-circle" label="Thêm liên hệ" outlined />
                </template>
            </DataTable>
            <div class="col-12 flex justify-content-end pr-0 mt-2">
                <Button icon="pi pi-save" label="Lưu thông tin"/>
            </div>
            <h4 class="font-semibold border-bottom-1 border-300 text-700 pb-2 mb-4">Địa chỉ xuất hoá đơn</h4>
            <div class="formgrid grid">
                <div class="field col-4">
                    <label for="firstname2">Họ</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-8">
                    <label for="firstname2">Tên</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12">
                    <label for="firstname2">Tên công ty (Không bắt buộc)</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12">
                    <label for="lastname2">Địa chỉ</label>
                    <InputText id="lastname2" class="w-full" type="text" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Tỉnh/Thành phố</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Quận/Huyện</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Xã/Phường</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-6">
                    <label for="firstname2">Email</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-6">
                    <label for="firstname2">Số điện thoại</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12 flex justify-content-end mt-2">
                    <Button icon="pi pi-save" label="Lưu thông tin"/>
                </div>
            </div>
            <h4 class="font-semibold border-bottom-1 border-300 text-700 pb-2 mb-4">Địa chỉ giao hàng</h4>
            <div class="formgrid grid">
                <div class="field col-4">
                    <label for="firstname2">Họ</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-8">
                    <label for="firstname2">Tên</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12">
                    <label for="firstname2">Tên công ty (Không bắt buộc)</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12">
                    <label for="lastname2">Địa chỉ</label>
                    <InputText id="lastname2" class="w-full" type="text" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Tỉnh/Thành phố</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Quận/Huyện</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-4">
                    <label for="lastname2">Xã/Phường</label>
                    <Dropdown class="w-full" />
                </div>
                <div class="field col-6">
                    <label for="firstname2">Email</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-6">
                    <label for="firstname2">Số điện thoại</label>
                    <InputText id="firstname2" class="w-full" type="text" />
                </div>
                <div class="field col-12 flex justify-content-end mt-2">
                    <Button icon="pi pi-save" label="Lưu thông tin"/>
                </div>
            </div>
        </div>
        <div class="col-12"></div>
    </div>
</template>

<script setup>
import Textarea from 'primevue/textarea';
import { ref } from 'vue';
const lienHeData = ref([]);
const appendContact = () => {
    // if(!lienHeData.value[lienHeData.value.length - 1]?.notSave)
    lienHeData.value.push({
        tenNguoiLienHe: '',
        chucVu: '',
        soDienThoai1: '',
        soDienThoai2: '',
        email: '',
        notSave: true
    });
};
</script>

<style>
._InputText__ {
    width: 100% !important;
    height: 100% !important;
    display: block;
    border-radius: 50%;
}
</style>
