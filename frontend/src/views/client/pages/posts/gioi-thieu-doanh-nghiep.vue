<template>
    <div class="m-8 py-5">
        <h4 class="mb-5 font-bold">GIỚI THIỆU DOANH NGHIỆP</h4>
        <h5 class="mb-3">
            Áp dụng cho hệ thống website:
            <a class="font-bold text-primary" href="https://portal.apsaigonpetro.com"> https://portal.apsaigonpetro.com </a>
        </h5>
        <div>
            <p>
                Công ty Cổ phần AP SAIGON PETRO là doanh nghiệp chuyên sản xuất và phân phối các sản phẩm dầu nhớt, hóa chất và dịch vụ hỗ trợ kỹ thuật trên toàn quốc. <br />
                Chúng tôi hoạt động theo giấy phép đăng ký kinh doanh số 0305715556 do Sở Kế hoạch và Đầu tư TP.HCM cấp lần đầu ngày 06 tháng 5 năm 2008
            </p>
            <ul>
                <li>Mã số thuế: 0305715556</li>
                <li>Địa chỉ: Lầu 1, 6B Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP. HCM</li>
                <li>Hotline: 1900 0104</li>
                <li>Email: dvcskh@apsaigonpetro.com.vn</li>
            </ul>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
li {
    margin-bottom: 8px;
}
</style>
