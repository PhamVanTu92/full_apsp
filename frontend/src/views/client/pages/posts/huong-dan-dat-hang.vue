<template>
    <div class="m-8">
        <h4 class="font-bold">HƯỚNG DẪN ĐẶT HÀNG VÀ XÁC NHẬN ĐƠN</h4>
        <h5 class="text-lg mb-3">
            Áp dụng cho hệ thống website:
            <a class="font-bold text-primary" href="https://portal.apsaigonpetro.com"> https://portal.apsaigonpetro.com </a>
        </h5>
        <h5 class="font-bold">Công ty Cổ phần AP SAIGON PETRO</h5>

        <ul class="dash-style">
            <li>Người mua đăng nhập tài khoản, chọn sản phẩm và đưa vào giỏ hàng.</li>
            <li>Kiểm tra thông tin, xác nhận điều khoản, kiểm tra thông tin công nợ, gửi đơn hàng.</li>
            <li>Đơn hàng sẽ được kiểm tra và xác nhận qua email/số điện thoại đăng ký.</li>
            <li>Người mua có thể chỉnh sửa đơn hàng trước khi được xác nhận.</li>
        </ul>

        <div class="mt-8">
            <h4 class="font-bold">Đăng ký tài khoản tổ chức/đại lý</h4>
            <p class="mt-3">
                Đối với khách hàng tổ chức hoặc đại lý, mỗi tài khoản sẽ gắn với một MST duy nhất. APSAIGONPETRO có quyền từ chối hoặc vô hiệu hóa các tài khoản có dấu hiệu trùng lặp, gian lận hoặc không minh bạch về danh tính pháp lý.
            </p>
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
h6 {
    margin-bottom: 3px;
}
.number-style {
    list-style-type: decimal;
}

.number-style > li {
    margin-bottom: 8px;
}

.dash-style {
    list-style-type: none;
}
.dot-style {
    list-style-type: disc;
}
.dot-style > li {
    margin-bottom: 4px;
}

.dash-style > li::before {
    content: '-';
    margin-right: 10px;
    font-size: 20px;
}
</style>
