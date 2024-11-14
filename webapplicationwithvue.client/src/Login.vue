<template>
  <br>
  <div>
    <form @submit.prevent="login">
      <input type="text" v-model="username" placeholder="Username"><br>
      <input type="password" v-model="password" placeholder="Password"><br>
      <button type="submit">Login</button>
    </form>
  </div>
</template>

<script>
  import axios from "axios";

  export default {
    data() {
      return {
        username: '',
        password: '',
        error: null
      }
    },
    methods: {
      async login() {
        try {
          const response = await axios.post("https://localhost:7084/Auth/login", {
            username: this.username,
            password: this.password
          });


          localStorage.setItem("token", response.data.access_token);
          localStorage.setItem("refresh", response.data.refresh_token);
          //this.$router.push('/weather');
        } catch (error) {
          console.log(error);
        }
      }
    }
  }
</script>
