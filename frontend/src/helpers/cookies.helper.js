const cookie = {
    // Đọc giá trị của cookie theo tên
    get: function (name) {
        let matches = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([.$?*|{}()[\]\\/+^])/g, '\\$1') + '=([^;]*)'));
        return matches ? decodeURIComponent(matches[1]) : undefined;
    },
    // Thiết lập giá trị của cookie theo tên, giá trị và thời gian hết hạn (tính bằng ngày)
    set: function (name, value, days) {
        let expires = '';
        if (days) {
            let date = new Date();
            date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
            expires = '; expires=' + date.toUTCString();
        }
        if (this.get(name) != undefined) this.clear(name);
        document.cookie = name + '=' + (value || '') + expires + '; path=/';
    },

    clear: function (name) {
        document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    }
};

export default cookie;
