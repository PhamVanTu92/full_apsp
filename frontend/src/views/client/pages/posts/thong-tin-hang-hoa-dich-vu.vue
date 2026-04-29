<template>
    <div class="m-8">
        <h4 class="font-bold">THÔNG TIN HÀNG HÓA VÀ DỊCH VỤ</h4>
        <h5 class="text-lg mb-3">
            Áp dụng cho hệ thống website:
            <a class="font-bold text-primary" href="https://portal.apsaigonpetro.com"> https://portal.apsaigonpetro.com </a>
        </h5>
        <h5 class="font-bold">Công ty Cổ phần AP SAIGON PETRO</h5>

        <ul class="dash-style">
            <li>Sản phẩm hiển thị gồm tên gọi, mã hàng, quy cách, giá bán.</li>
            <li>Một số sản phẩm có đính kèm tài liệu kỹ thuật riêng.</li>
            <li>Thông tin được đồng bộ từ hệ thống quản lý hàng hóa của AP Saigonpetro và cập nhật thường xuyên.</li>
        </ul>

        <div class="mt-8">
            <h4 class="font-bold">Lưu ý về độ chính xác mô tả hàng hóa</h4>
            <p class="mt-3">
                APSAIGONPETRO cam kết cung cấp mô tả sản phẩm chính xác nhất. Tuy nhiên, có thể có sai sót nhỏ trong hình ảnh hoặc mô tả kỹ thuật, và chúng tôi không chịu trách nhiệm nếu sự sai lệch không ảnh hưởng đến bản chất sản phẩm. Khách hàng
                nên xác nhận lại trước khi đặt hàng số lượng lớn.
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
