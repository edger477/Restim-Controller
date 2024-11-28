
const API_BASE_URL = '/api';

let authKey = '';

function setAuthKey(key) {
    authKey = key;
}

async function callApi(endpoint, body, { method = 'GET', headers = {} } = {}) {
    const url = `${API_BASE_URL}${endpoint}`;

    const fetchOptions = {
        method,
        headers: {
            'Content-Type': 'application/json',
            'AUTHKEY': authKey,
            ...headers, // Merge additional headers if provided
        },
    };

    if (body) {
        fetchOptions.body = JSON.stringify(body); // Stringify the body if present
    }

    try {
        const response = await fetch(url, fetchOptions);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json(); // Assuming JSON response
    } catch (error) {
        console.error('Error making API request:', error);
        throw error; // Re-throw the error for handling in calling code
    }
}

const apiClient = {
    get: callApi,
    post: (endpoint, body) => callApi(endpoint, body, { method: 'POST' })
};



// API functions
const login = async (key) => {
    try {
        const response = await apiClient.post('/auth/login', { authKey: key });
        if (response.success) { authKey = key };
        setAuthKey(key);
        return response;
    } catch (error) {
        console.error("Error fetching audio sessions:", error);
        throw error;
    }
};

// API functions
const getInstances = async () => {
    try {
        const response = await apiClient.get('/restim/instances');
        return response;
    } catch (error) {
        console.error("Error fetching audio sessions:", error);
        throw error;
    }
};

const setVolume = async (instance, volume) => {
    try {
        const response = await apiClient.post('/restim/volume', { Id: instance.id, InstanceName: instance.name, Volume: volume });
        return response.data;
    } catch (error) {
        console.error("Error setting volume:", error);
        throw error;
    }
};

const setAxis = async (instance, axis) => {
    try {
        const response = await apiClient.post(`/restim/${instance.id}/axis`, axis);
        return response.data;
    } catch (error) {
        console.error("Error setting volume:", error);
        throw error;
    }
}

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

const pause = async (instance, durationSeconds) => {
    try {
        const response = await apiClient.post(`/restim/${instance.id}/pause/${durationSeconds}`, { Id: instance.id, durationSeconds: durationSeconds });
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
const resume = async (instance) => {
    try {
        const response = await apiClient.post(`/restim/volume`, { Id: instance.id, InstanceName: instance.name, Volume: instance.originalVolume });
        return response.data;
    } catch (error) {
        console.error("Error muting session:", error);
        throw error;
    }
};

const api = {
    setAuthKey: setAuthKey,
    login: login,
    getInstances: getInstances,
    setVolume: setVolume,
    setAxis: setAxis,
    spike: spike,
    pause: pause,
    muteSession: muteSession,
    resume: resume
};

export default api;