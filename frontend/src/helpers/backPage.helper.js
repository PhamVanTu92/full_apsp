export const backPage = (router, route) => {
    if(route.name == 'client-order-detail'){
        router.replace({ name: 'hisPur' });
    }else if(route.name == 'hisPurNET-detail'){
        router.replace({ name: 'hisPurNET' });
    }else{
        router.back();
    }
};