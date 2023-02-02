import {HubConnectionBuilder,LogLevel} from '@microsoft/signalr'
import settings from './settings';
import {createContext, useContext, useEffect, useRef, useState} from 'react';

const ProcessesConsoleContext = createContext([]);
const maxProcessesInCache = 100;
const maxConsoleLinesInCache = 100;

const ProcessesConsoleProvider = ({children}) => {
  const [tryRefreshConnection, setTryRefreshConnection] = useState(true);
  const [consoleMessageCache, setConsoleMessageCache] = useState({});
  const consoleMessageCacheRef = useRef({});

  useEffect(() => {
    const connection = new HubConnectionBuilder()
        .withUrl(`${settings.workflowUrl}/processConsole/`)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Debug)
        .build();
    const connect = async () => {
      connection.on('ReceiveMessage', function (message) {
        const cache = Object.assign({}, consoleMessageCacheRef.current);
        let processCache = cache[message.processId];
        if (!processCache) {
          const keys = Object.keys(cache);
          if (keys.length >= maxProcessesInCache) {
            delete cache[keys[0]];
          }
          processCache = cache[message.processId] = [];
        } else {
          processCache = cache[message.processId] = [...processCache];
        }
        processCache.unshift({
          date: new Date(),
          message: message.message.replaceAll('\\n', '\n')
        });
        processCache = processCache.slice(0, maxConsoleLinesInCache);
        consoleMessageCacheRef.current = cache;
        setConsoleMessageCache(consoleMessageCacheRef.current);
      });
      await connection.start();
    }
    const disconnect = async () => {
      connection.off('ReceiveMessage');
      await connection.stop();
    }
    connect();
    return disconnect;
  }, [tryRefreshConnection])

  return <ProcessesConsoleContext.Provider value={consoleMessageCache}>{children}</ProcessesConsoleContext.Provider>
}

const useProcessesConsoleContext = () => useContext(ProcessesConsoleContext);

export  {ProcessesConsoleProvider, useProcessesConsoleContext};