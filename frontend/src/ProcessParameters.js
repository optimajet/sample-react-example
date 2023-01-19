import {useEffect, useState} from "react";
import {Table, Input, Checkbox, Container, Button} from 'rsuite';

const {Column, HeaderCell, Cell} = Table;

const EditableInputCell = ({rowData, dataKey, onChange, idKey, readOnlyFn, ...props}) => {
    return <Cell {...props} style={{padding: '5px'}}>
        <Input readOnly={readOnlyFn(rowData)} value={rowData[dataKey]} onChange={value => {
            onChange && onChange(rowData[idKey], dataKey, value)
        }}/>
    </Cell>;
}

const EditableTextAreaCell = ({rowData, dataKey, onChange, idKey, ...props}) => {
    return <Cell {...props} style={{padding: '5px'}}>
        <Input as="textarea" rows={3} value={rowData[dataKey]} onChange={value => {
            onChange && onChange(rowData[idKey], dataKey, value)
        }}/>
    </Cell>;
}

const EditableCheckboxCell = ({rowData, dataKey, onChange, idKey, ...props}) => {
    return <Cell {...props} style={{padding: '5px'}}>
        <Checkbox checked={rowData[dataKey]} onChange={(_, checked) => {
            onChange && onChange(rowData[idKey], dataKey, checked)
        }}/>
    </Cell>;
}

const ButtonCell = ({rowData,idKey,onClick,text,hideFn,...props}) => {
    return <Cell {...props} style={{padding: '5px'}}>
        {!hideFn(rowData) &&
        <Button onClick={() => {
            onClick && onClick (rowData[idKey])
        }}>{text}</Button>}
    </Cell>;
}

const defaultParameter = {
    name: "parameter", value: "", persist: true, isRequired: false
}

const ProcessParameters = ({onParametersChanged, defaultParameters, ...props}) => {
    const idKey = "name";
    const [data, setData] = useState(defaultParameters ?? []);
    const [autoHeight, setAutoHeight] = useState(false);

    useEffect(() => {
        onParametersChanged(data);
    },[data]);

    useEffect(() => {
        setAutoHeight(true);
    },[])

    const onChange = (id, key, value) => {
        const nextData = Object.assign([], data);
        let updated = nextData.find(item => item[idKey] === id);
        if (key === idKey) {
            const updatedClone = {...updated};
            updatedClone[key] = value;
            updated[key] = rewriteDuplicatedKey(nextData, updatedClone)[key];
        } else {
            updated[key] = value;
        }
        setData(nextData);
    };

    const onAdd = () => {
        const nextData = Object.assign([], data);
        const dataCount = nextData.length;
        const newParameter = {...defaultParameter};
        newParameter.name = `${newParameter.name}_${dataCount + 1}`;
        nextData.push(rewriteDuplicatedKey(nextData, newParameter));
        setData(nextData);
    }

    const onDelete = (id) => {
       let nextData = Object.assign([], data);
       const index = nextData.findIndex(item => item[idKey] === id);
       nextData = nextData.slice(0,index).concat(nextData.slice(index + 1));
       setData(nextData);
    }

    const rewriteDuplicatedKey = (data, newItem) => {
        if (data.find(item => item[idKey] === newItem[idKey])) {
            const split = newItem[idKey].split("_");
            const last = parseInt(split[split.length - 1]);
            
            if (!isNaN(last)) {
                if (split.length > 1) {
                    newItem[idKey] = `${split.slice(0, split.length - 1).join("_")}_${last + 1}`;
                } else {
                    newItem[idKey] = `${last + 1}`;
                }
            } else {
                newItem[idKey] = `${newItem[idKey]}_1`;
            }

            return rewriteDuplicatedKey(data, newItem);
        }

        return newItem;
    }

    return <Container style={{overflow: 'hidden'}}>
        <Table data={data} rowHeight={87} height={(87*3 + 40)} autoHeight={autoHeight} >
            <Column flexGrow={2}>
                <HeaderCell>Name</HeaderCell>
                <EditableInputCell dataKey={"name"} idKey={idKey} onChange={onChange} readOnlyFn={(rowData) => rowData["isRequired"]}></EditableInputCell>
            </Column>
            <Column flexGrow={4}>
                <HeaderCell>Value</HeaderCell>
                <EditableTextAreaCell dataKey={"value"} idKey={idKey} onChange={onChange}></EditableTextAreaCell>
            </Column>
            <Column flexGrow={1}>
                <HeaderCell>Persist</HeaderCell>
                <EditableCheckboxCell dataKey={"persist"} idKey={idKey} onChange={onChange}></EditableCheckboxCell>
            </Column>
            <Column flexGrow={1}>
                <HeaderCell>Delete</HeaderCell>
                <ButtonCell idKey={idKey} text={"Delete"} onClick={onDelete} hideFn={(rowData) => rowData["isRequired"]}/>
            </Column>
        </Table>
        <Button onClick={onAdd}>Add</Button>
    </Container>;
}

export default ProcessParameters;