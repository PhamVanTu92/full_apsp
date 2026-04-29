import { useCartStore } from '../Pinia/cart';

export default function getCart({ next }) {
    const cartStore = useCartStore();
    cartStore
        .getCart()
        .then(() => {
        })
        .catch((error) => {
            console.error(error);
        });

    return next();
}
