import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'home',
    component: Home
  },
  {
    path: '/AllOrders',
    name: 'allorders',
    component: () => import('../views/AllOrders.vue')
  },
  {
    path: '/NewOrder',
    name: 'neworder',
    component: () => import('../views/NewOrder.vue')
  }
]

const router = new VueRouter({
  routes
})

export default router
