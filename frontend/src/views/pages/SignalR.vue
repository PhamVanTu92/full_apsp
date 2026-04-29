<template>
    <div>
        <h1>Check SignalR</h1>
        <ul>
            <li v-for="(message, index) in messages" :key="index">{{ message }}</li>
        </ul>
        <Input type="text" v-model="message" />
        <Button label="Send" @click="sendMessage" />
        <Button label="Call api" @click="callApi" />
    </div>
</template>

<script setup>
import { ref, onMounted, onBeforeMount } from 'vue';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/Pinia/auth';
import API from '@/api/api-main';
const authStore = useAuthStore();
const messages = ref([]);

// Tạo một kết nối mới đến Hub của SignalR
// Khi VITE_APP_WS_HOST rỗng (dev proxy), dùng đường dẫn tương đối
const _wsBase = import.meta.env.VITE_APP_WS_HOST || '';
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${_wsBase}/notificationHubs`
      ,{
        accessTokenFactory: () => authStore.token,
        skipNegotiation: true,
        transport: 1
      }
    )
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.None)
    .build();

// Hàm để bắt đầu kết nối
const startConnection = async () => {
    try {
        await connection.start();


    } catch (error) {
        console.error('SignalR connection failed: ', error);
        setTimeout(startConnection, 5000); // Thử kết nối lại sau 5 giây
    }
};

const callApi = async () => {
  const res = await API.get(`Fee/feeByCus/1`);
  const res2 = await API.get(`Fee/feeByCusMess/1`);



}

onBeforeMount(async () => {
  await startConnection();
});

const message = ref('');
const sendMessage = () => {
    connection.invoke('SendNotification', message.value).catch(err => console.error(err));
};


//Hàm để nhận thông báo khi server gửi thông báo
connection.on('ReceiveNotification',  msg => {

  messages.value.push(msg);
});


</script>
