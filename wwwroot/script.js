import api from './api.js';
import { Device, Spike } from './models.js';

const app = Vue.createApp({
  data() {
    return {
      authKey: document.cookie['authKey'] || '',
      isLoggedIn: false,
      devices: []
    };
  },
  methods: {
    async login() {
      try {
        const result = await api.login(this.authKey);
        if (result.success) {
          document.cookie = `authKey=${this.authKey}; path=/;`;
          this.isLoggedIn = true;
          this.startVolumeRefresh();
        } else {
          alert('Invalid Auth Key');
        }
      } catch (error) {
        alert('Login failed');
      }
    },
    async fetchVolumes() {
      try {
        if (this.fetchVolumes.fetching) return;
        this.fetchVolumes.fetching = true;
        const response = await api.getDevices();
        var devices = response.devices.map(d => new Device(d));

        this.devices = this.devices.filter((local) => this.devices.some((remote) => local.id === remote.id));
        this.devices.forEach(device => device.volume = devices.find(d => d.id == device.id).volume);

        var missing = devices.filter(
          (local) => !this.devices.some((remote) => local.id === remote.id)
        );
        this.devices = this.devices.concat(missing);



        // this.devices.forEach(device => {
        //   const deviceData = response.devices.find(d => d.id === device.id);
        //   if (deviceData) device.volume = deviceData.volume;
        // });
        this.fetchVolumes.fetching = false;
        setTimeout(this.fetchVolumes, 100);
      } catch (error) {
        console.error('Failed to fetch volumes', error);
      }
    },
    async adjustVolume(device, amount) {
      try {
        device.fetching = true;
        const response = await api.setVolume(device, device.volume + amount);
        console.log(response);
        device.fetching = false;
        this.fetchVolumes();
      } catch (error) {
        console.error('Failed to adjust volume', error);
      }
    },
    async spikeVolume(device, spike) {
      try {
        device.fetching = true;
        const response = await api.spike(device, spike);
        console.log(response);
        setTimeout(function () {
          device.fetching = false;
        }, (spike.size + spike.period * 1000) * spike.repeat + 1000);
      } catch (error) {
        console.error('Failed to spike volume', error);
      }
    },
    async pauseVolume(device, duration) {
      try {
        device.originalVolume = device.volume;
        if(duration === 0) { device.paused = true; }
        device.fetching = true;
        const response = await api.pause(device, duration);
        console.log(response);
        setTimeout(function () {
          device.fetching = false;
        }, duration * 1000 + 1000);
      } catch (error) {
        console.error('Failed to pause volume', error);
      }
    },
    async resume(device) {
      try {
        device.paused = false;
        device.fetching = true;
        const response = await api.resume(device);
        console.log(response);
        setTimeout(function () {
          device.fetching = false;
        }, 5000);
      } catch (error) {
        console.error('Failed to pause volume', error);
      }
    },
    getAuthKey() {
      return document.cookie.split('; ').find(row => row.startsWith('authKey='))?.split('=')[1] || '';
    },
    startVolumeRefresh() {
      // Start refreshing the volume every second
      this.fetchVolumes(); // Initial fetch
    }
  },
  mounted() {
    if (this.getAuthKey()) {
      this.authKey = this.getAuthKey();
      api.setAuthKey(this.authKey);
      this.isLoggedIn = true;
      this.startVolumeRefresh();
    }
  }
});

app.mount('#app');
