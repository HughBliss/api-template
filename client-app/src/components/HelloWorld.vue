<template>
  <div class="hello">
     <input type="number" v-model="hello" />
    <h1>{{ msg }}</h1>
<h3>{{hello}}! = {{result}}</h3>
  </div>
</template>

<script>
export default {
  name: 'HelloWorld',
  props: {
    msg: String
  },
  data: () => ({
    hello: 1,
    result: ''
  }),
  methods: {
    execFact (num) {
      this.$axios.get(`/api/execFact/${num}`).then(response => {

        this.result = response.data.split(' ')[0]
      })
    }
  },
  watch: {
    hello (val) {
      this.execFact(val)
    }
  }
  // computed: {
  //               factorial() {
  //                   let temp = ''
  //                   for (let i = 1; i <= this.hello; i++) {
  //                       temp += i == this.hello ? i : `${i}*`
  //                   }
  //                   return eval(temp)
  //               }
  //           },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
h3 {
  margin: 40px auto;
  max-width: 50vw;
   word-wrap: break-word;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
