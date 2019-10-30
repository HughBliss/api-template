<template>
  <div class="AllOrders">
    <table>
      <tbody>
        <tr>
          <td>ID</td>
          <td>Client Name</td>
          <td>Product Name</td>
        </tr>
        <tr v-for="order in orders" :key="order.id">
          <td>{{order.id}}</td>
          <td>{{order.clientName}}</td>
          <td>{{order.productName}}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  data: () => ({
    orders: []
  }),
  created() {
    this.$axios.get("https://localhost:5001/api/orders").then(res => {
      this.orders = res.data;
      console.log(res);
    });
  }
};
</script>