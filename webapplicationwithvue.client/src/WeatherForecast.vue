<template>
  <div>
    <h2>Weather Forecast</h2>
    {{weatherForecast}}
  </div>
</template>

<script>
  import axios from "axios";

  export default {
    name: "WeatherForecast",
    data() {
      return {
        weatherForecast: null
      }
    },
   async mounted() {

      axios.interceptors.request.use(config => {
        const token = localStorage.getItem("token");
        console.log("Request token: ", token);
        if (token) {
          config.headers['Authorization']
            = `Bearer ${token}`;
        }
        console.log("Authorization header set: ", config.headers['Authorization']);
        return config;
      });
    await this.getWeatherForecast();
    },
    methods: {
      getWeatherForecast() {
        const response = axios.get("https://localhost:7084/WeatherForecast")
          .then(response => {
            console.log(response);
            this.weatherForecast = response.data;
          })
          .catch(async error => {
            console.log(error);
            if (error.response && error.response.status === 401) {
              const refreshToken = localStorage.getItem('refresh');
              const rs = await axios.post('https://localhost:7084/Auth/refresh', { tokenString: refreshToken })
                .then(response => {
                  var token = response.data.access_token;
                  console.log("token", token);
                  axios.defaults.headers.common["Authorization"] = "Bearer " + token;
                  console.log("New token set: ", token);
                  //setTimeout(this.getWeatherForecast, 1000);
                  
                })
            }
            this.weatherForecast = "401 Unauthorized!";
          });
      }
    }
  }
</script>
