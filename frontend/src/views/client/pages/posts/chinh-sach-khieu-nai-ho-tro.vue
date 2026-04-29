<template>
    <div class="m-8 py-5">
        <h4 class="mb-5 font-bold">CHÍNH SÁCH KHIẾU NẠI & HỖ TRỢ KHÁCH HÀNG</h4>
        <h5 class="mb-3">
            Áp dụng cho hệ thống website:
            <a class="font-bold text-primary" href="https://portal.apsaigonpetro.com"> https://portal.apsaigonpetro.com </a>
        </h5>
        <h5 class="font-bold mb-4">Công ty Cổ phần AP SAIGON PETRO</h5>

        <ul class="number-style">
            <li>
                <h6>KÊNH TIẾP NHẬN KHIẾU NẠI & HỖ TRỢ</h6>
                <p class="mb-2">Khách hàng có thể liên hệ trực tiếp với Công ty Cổ phần AP SAIGON PETRO thông qua các kênh chính thức sau:</p>
                <div class="ml-4 my-3">
                    <div class="mb-2">Hotline: 1900 0104 (trong giờ hành chính)</div>
                    <div class="mb-2">Email: dvcskh@apsaigonpetro.com.vn</div>
                    <div class="mb-2">Địa chỉ tiếp nhận trực tiếp: Lầu 1, 6B Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh</div>
                </div>
                <p class="mt-2"><strong>Bộ phận tiếp nhận và xử lý:</strong> Bộ phận Dịch vụ Chăm sóc Khách hàng (DVCSKH)</p>
            </li>

            <li>
                <h6>THỜI GIAN PHẢN HỒI VÀ XỬ LÝ KHIẾU NẠI</h6>
                <div class="ml-4 my-3">
                    <div class="mb-2">Thời gian phản hồi ban đầu: Trong vòng 24 giờ làm việc kể từ thời điểm tiếp nhận.</div>
                    <div class="mb-2">Thời gian xử lý hoàn tất: Tối đa 07 ngày làm việc.</div>
                    <div class="mb-2">Trường hợp phức tạp hoặc cần xác minh nội bộ, thời gian xử lý có thể kéo dài nhưng không quá 14 ngày làm việc, kèm thông báo lý do cụ thể đến khách hàng.</div>
                </div>
            </li>

            <li>
                <h6>NGUYÊN TẮC GIẢI QUYẾT KHIẾU NẠI</h6>
                <div class="ml-4 my-3">
                    <div class="mb-2">Công ty cam kết bảo mật thông tin người gửi khiếu nại.</div>
                    <div class="mb-2">Mọi thông tin khiếu nại đều được lưu trữ, theo dõi và đánh giá nội bộ nhằm cải thiện chất lượng dịch vụ.</div>
                    <div class="mb-2">Trường hợp phát sinh tranh chấp, Công ty sẽ phối hợp với các bên liên quan (nếu có) để giải quyết theo quy định pháp luật hiện hành.</div>
                    <div class="mb-2">Trong mọi trường hợp, Công ty sẽ luôn ưu tiên phương án thỏa thuận trên tinh thần thiện chí với khách hàng.</div>
                </div>
            </li>

            <li>
                <h6>PHẠM VI ÁP DỤNG</h6>
                <div class="ml-4 my-3">
                    <div class="mb-2">Chính sách này áp dụng đối với tất cả khách hàng đã giao dịch, sử dụng dịch vụ và sản phẩm qua hệ thống Portal.</div>
                    <div class="mb-2">Các nội dung hỗ trợ ngoài phạm vi giao dịch sẽ được xem xét xử lý tùy theo từng trường hợp cụ thể, đảm bảo quyền lợi khách hàng.</div>
                </div>
            </li>
        </ul>

        <div class="mt-8">
            <h4 class="font-bold">Quy trình xử lý khiếu nại theo cấp</h4>
            <p class="mt-3 mb-3">Quy trình xử lý khiếu nại gồm 3 cấp độ:</p>
            <div class="ml-4">
                <div class="mb-2">- Cấp 1: Tiếp nhận qua kênh CSKH</div>
                <div class="mb-2">- Cấp 2: Chuyển đến bộ phận kỹ thuật, tài chính hoặc pháp lý</div>
                <div class="mb-2">- Cấp 3: Ban điều hành xem xét nếu khiếu nại không được giải quyết sau 5 ngày.</div>
            </div>
            <p class="mt-3">Mọi khiếu nại đều được lưu hồ sơ để phục vụ kiểm tra nội bộ và cơ quan chức năng khi cần.</p>
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
    margin-bottom: 8px;
    font-weight: bold;
}

.number-style {
    list-style-type: upper-roman;
}

.number-style > li {
    margin-bottom: 16px;
}

.dash-style {
    list-style-type: none;
}

.dash-style > li::before {
    content: '-';
    margin-right: 10px;
    font-size: 20px;
}

.dash-style > li {
    margin-bottom: 6px;
}
</style>
