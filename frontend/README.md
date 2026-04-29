# Saigon Petro Procurement - Hệ thống Quản lý Mua hàng

Đây là dự án front-end cho hệ thống quản lý mua hàng của Saigon Petro, được xây dựng bằng Vue.js và PrimeVue.

## Giới thiệu

Ứng dụng này cung cấp một giao diện hiện đại và hiệu quả để quản lý các quy trình mua hàng, bao gồm tạo đơn hàng, quản lý nhà cung cấp, theo dõi trạng thái đơn hàng, và nhiều tính năng khác.

## Tính năng chính

-   Quản lý và tạo mới đơn hàng (Purchase Order).
-   Hiển thị danh sách và chi tiết đơn hàng.
-   Lọc và tìm kiếm đơn hàng nâng cao.
-   Xuất dữ liệu ra file Excel.
-   Quản lý phí lưu kho.
-   Giao diện đa ngôn ngữ (Tiếng Việt, Tiếng Anh).
-   Phân quyền và xác thực người dùng.

## Công nghệ sử dụng

-   **Framework:** [Vue.js 3](https://vuejs.org/) (Composition API)
-   **Build Tool:** [Vite](https://vitejs.dev/)
-   **UI Library:** [PrimeVue](https://www.primevue.org/)
-   **Quản lý trạng thái:** [Pinia](https://pinia.vuejs.org/)
-   **Routing:** [Vue Router](https://router.vuejs.org/)
-   **Đa ngôn ngữ:** [Vue I18n](https://vue-i18n.intlify.dev/)
-   **Styling:** [PrimeFlex](https://www.primefaces.org/primeflex/) & CSS/SCSS

## Hướng dẫn cài đặt và sử dụng

### Yêu cầu hệ thống

-   [Node.js](https://nodejs.org/) (phiên bản 18.x trở lên)
-   [npm](https://www.npmjs.com/) hoặc [yarn](https://yarnpkg.com/)

### Các bước cài đặt

1.  **Clone repository về máy:**

    ```bash
    git clone https://github.com/TanKmt113/saigon_petro_procurement.git
    cd saigon_petro_procurement
    ```

2.  **Cài đặt các dependencies:**

    ```bash
    npm install
    ```

3.  **Cấu hình môi trường:**
    Sao chép nội dung từ file `.env.development` hoặc `.env.production` và tạo một file `.env.local` để cấu hình các biến môi trường cho máy local của bạn.
    Ví dụ:

    ```
    VITE_APP_API_URL=http://localhost:5000/api
    ```

4.  **Chạy ứng dụng ở chế độ development:**
    ```bash
    npm run dev
    ```
    Ứng dụng sẽ chạy tại địa chỉ `http://localhost:5173` (hoặc một cổng khác nếu cổng 5173 đã được sử dụng).

## Các tập lệnh (Scripts) có sẵn

-   `npm run dev`: Chạy ứng dụng ở chế độ development với hot-reload.
-   `npm run build:dev`: Build ứng dụng cho môi trường development.
-   `npm run build:uat`: Build ứng dụng cho môi trường UAT.
-   `npm run preview`: Xem trước bản build production tại local.
-   `npm run lint`: Kiểm tra và tự động sửa lỗi code style theo Eslint.

## Author : An Tú

## BaseCode : Tuấn Vi
