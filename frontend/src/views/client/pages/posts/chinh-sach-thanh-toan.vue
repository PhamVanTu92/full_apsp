<template>
    <div class="m-8">
        <h4 class="font-bold">CHÍNH SÁCH THANH TOÁN</h4>

        <ul class="number-style">
            <li>
                <h6>Thanh toán trực tuyến qua cổng thanh toán OnePay</h6>
                <ul class="dot-style">
                    <li>Áp dụng cho thẻ ATM nội địa, thẻ tín dụng/quốc tế (VISA, MasterCard), và mã QR thanh toán.</li>
                    <li>Khi lựa chọn hình thức thanh toán này, khách hàng sẽ được chuyển hướng đến trang thanh toán bảo mật của OnePay – đối tác cổng thanh toán được cấp phép.</li>
                    <li>Mọi giao dịch được xử lý trực tiếp trên hệ thống của OnePay theo tiêu chuẩn bảo mật PCI DSS.</li>
                    <li>APSP không lưu trữ, xử lý hoặc truy cập vào bất kỳ thông tin thẻ thanh toán nào của khách hàng.</li>
                </ul>
            </li>

            <li>
                <h6>Thanh toán bằng chuyển khoản ngân hàng</h6>
                <ul class="dot-style">
                    <li>Áp dụng cho khách hàng thực hiện thanh toán qua tài khoản ngân hàng.</li>
                    <li>Khách hàng sẽ thực hiện chuyển khoản sau thời điểm đơn hàng được xác nhận.</li>
                    <li>Việc thanh toán được thực hiện giữa khách hàng và tổ chức cung cấp dịch vụ ngân hàng mà khách hàng sử dụng.</li>
                </ul>
            </li>

            <li>
                <h6>Thanh toán theo hạn mức tín chấp hoặc bảo lãnh ngân hàng</h6>
                <ul class="dot-style">
                    <li>Áp dụng riêng cho khách hàng doanh nghiệp có hợp đồng thương mại với Công ty Cổ phần AP SAIGON PETRO.</li>
                    <li>Đơn hàng sẽ được trừ trực tiếp vào hạn mức tín chấp còn hiệu lực hoặc dựa trên giá trị thư bảo lãnh ngân hàng được ghi nhận.</li>
                    <li>Trường hợp công nợ vượt hạn mức hoặc thư bảo lãnh hết hiệu lực, hệ thống sẽ từ chối đơn hàng hoặc yêu cầu thay đổi phương thức thanh toán phù hợp.</li>
                </ul>
            </li>
        </ul>

        <h6 class="font-bold mt-4">Lưu ý:</h6>
        <ul class="dot-style">
            <li>Mọi đơn hàng yêu cầu thanh toán phải được hoàn tất trong vòng 72 giờ kể từ thời điểm xác nhận đơn hàng bởi bộ phận DVCSKH.</li>
            <li>Trường hợp khách hàng không hoàn tất thanh toán trong thời gian trên, đơn hàng sẽ bị hủy với lý do "Khách hàng không thanh toán đúng hạn".</li>
            <li>Mỗi phương thức thanh toán sẽ được hệ thống ghi nhận tại thời điểm xác nhận đơn, làm căn cứ xử lý đơn hàng và đối chiếu nội bộ.</li>
        </ul>
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
