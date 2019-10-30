<template>
  <div class="NewOrder">
    <!-- <h1>This is an new order page</h1>
    <button @click="sendOrder">отправить ордер</button>
    -->
    <form @submit.prevent="sendOrder">
      <div class="input-group">
        <lable class="input-lable">Ваше имя</lable>
        <input type="text" class="input-input" v-model="order.clientName" />
      </div>
      <div class="input-group">
        <lable class="input-lable">Продукт</lable>
        <input type="text" class="input-input" v-model="order.productName" />
      </div>
      <button type="submit">отправить</button>
    </form>
    <h3>{{message}}</h3>
  </div>
</template>

<script>
export default {
  data: () => ({
    message: "",
    order: {
      clientName: "",
      productName: ""
    }
  }),
  methods: {
    sendOrder() {
      this.$axios
        .post("https://localhost:5001/api/orders", this.order, {
          headers: {
            "Content-Type": "application/json"
          }
        })
        .then(res => {
          console.log(res.data);
          this.message =
            "продукт " + res.data.productName + " успешно добавлен";
          this.order = {
            clientName: "",
            productName: ""
          };
        });
    }
  }
};
</script>