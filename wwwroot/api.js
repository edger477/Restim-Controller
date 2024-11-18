import axios from 'https://cdn.skypack.dev/axios';

const API_BASE_URL = '/api'; // Update to your API base URL
var authKey = '';
// Create an Axios instance with default configuration
const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        common: {
            'Content-Type': 'application/json',
            headers: { 'AUTHKEY': authKey }
        },
    },
});


const setAuthKey = (key) => {
    apiClient.defaults.headers.common.AUTHKEY = key;
}
// API functions
const login = async (key) => {
    try {
        const response = await apiClient.post('/auth/login', { authKey: key });
        if (response.data.success) { authKey = key };
        setAuthKey(key);
        return response.data;
    } catch (error) {
        console.error("Error fetching audio sessions:", error);
        throw error;
    }
};

// API functions
const getDevices = async () => {
    try {
        const response = await apiClient.get('/volume/devices');
        return response.data;
    } catch (error) {
        console.error("Error fetching audio sessions:", error);
        throw error;
    }
};

const setVolume = async (device, volume) => {
    try {
        const response = await apiClient.post('/volume/set', { Id: device.id, DeviceName: device.soundCard, Volume: volume });
        return response.data;
    } catch (error) {
        console.error("Error setting volume:", error);
        throw error;
    }
};

const spike = async (device, spike) => {
    try {
        const response = await apiClient.post('/volume/spike',
            {
                Id: device.id,
                DeviceName: device.soundCard,
                Amount: spike.intensity,
                MillisecondsOn: spike.size,
                MillisecondsOff: Math.round((spike.period * 1000) - spike.size),
                RepeatCount: spike.repetitions
            });
        return response.data;
    } catch (error) {
        console.error("Error setting volume:", error);
        throw error;
    }
};

const pause = async (device, durationSeconds) => {
    try {
        const response = await apiClient.post('/volume/pause', { Id: device.id, DeviceName: device.soundCard, durationSeconds: durationSeconds });
        return response.data;
    } catch (error) {
        console.error("Error setting volume:", error);
        throw error;
    }
};
const muteSession = async (device, isMuted) => {
    try {
        const response = await apiClient.post('/volume/mute', { Id: device.id, DeviceName: device.soundCard, isMuted });
        return response.data;
    } catch (error) {
        console.error("Error muting session:", error);
        throw error;
    }
};
const resume = async (device) => {
    try {
        const response = await apiClient.post('/volume/resume', { Id: device.id, DeviceName: device.soundCard, Volume: device.originalVolume });
        return response.data;
    } catch (error) {
        console.error("Error muting session:", error);
        throw error;
    }
};

const api = {
    setAuthKey: setAuthKey,
    login: login,
    getDevices: getDevices,
    setVolume: setVolume,
    spike: spike,
    pause: pause,
    muteSession: muteSession,
    resume: resume
};

export default api;