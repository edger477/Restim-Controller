import api from './api.js';
import { createApp, reactive } from 'vue';
import { Instance, Spike } from './models.js';

const app = createApp({
  data() {
    return {
      authKey: document.cookie['authKey'] || '',
      isLoggedIn: false,
      instances: []
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
    async fetchInstances() {
      try {
        if (this.fetchInstances.fetching) return;
        this.fetchInstances.fetching = true;
        const response = await api.getInstances();
        var instances = response.instances.map(d => reactive(new Instance(d)));

        this.instances = this.instances.filter((local) => instances.some((remote) => local.id === remote.id));
        this.instances.forEach(instance => instance.updateFrom(instances.find(d => d.id == instance.id)));

        var missing = instances.filter(
          (local) => !this.instances.some((remote) => local.id === remote.id)
        )
          .map(d => reactive(new Instance(d)));
        this.instances = this.instances.concat(missing);


        this.fetchInstances.fetching = false;
        setTimeout(this.fetchInstances, 500);
      } catch (error) {
        console.error('Failed to fetch volumes', error);
      }
    },
    async updateAxis(instance, axis) {
      try {
        instance.fetching = true;
        const response = await api.setAxis(instance, axis);
        console.log(response);
        instance.fetching = false;
        this.fetchInstances();
      } catch (error) {
        console.error('Failed to adjust axis', error);
      }
    },
    async setVolume(instance, value) {
      try {
        instance.fetching = true;
        const response = await api.setVolume(instance, value);
        console.log(response);
        instance.fetching = false;
        this.fetchInstances();
      } catch (error) {
        console.error('Failed to adjust volume', error);
      }
    },
    async adjustVolume(instance, amount) {
      try {
        instance.fetching = true;
        const response = await api.setVolume(instance, instance.newVolume + amount);
        console.log(response);
        instance.fetching = false;
        this.fetchInstances();
      } catch (error) {
        console.error('Failed to adjust volume', error);
      }
    },
    async spikeVolume(instance, spike) {
      try {
        instance.fetching = true;
        const response = await api.spike(instance, spike);
        console.log(response);
        setTimeout(function () {
          instance.fetching = false;
        }, (spike.size + spike.period * 1000) * spike.repeat + 1000);
      } catch (error) {
        console.error('Failed to spike volume', error);
      }
    },
    async pauseVolume(instance, duration) {
      try {
        instance.originalVolume = instance.volume;
        if (duration === 0) { instance.paused = true; }
        const response = await api.pause(instance, duration);
        console.log(response);
        await this.fetchInstances();
        instance.fetching = true;
        setTimeout(function () {
          instance.fetching = false;
        }, duration * 1000 + 1000);
      } catch (error) {
        console.error('Failed to pause volume', error);
      }
    },
    async resume(instance) {
      try {
        instance.paused = false;
        instance.fetching = true;
        const response = await api.resume(instance);
        console.log(response);
        setTimeout(function () {
          instance.fetching = false;
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
      this.fetchInstances(); // Initial fetch
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
