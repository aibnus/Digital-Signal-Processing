# Digital-Signal-Processing
Digital Signal Processing is a windows application written in C# that mainly for plot graphic signals from file or directly from serial monitor. Or just simply read, write, plot data from serial monitor.

## Features
- Plot data from .csv or .txt file.
- Use Discrete Fourier Transform to derive a frequency-domain (spectral) representation of the signal.
- Use many different filtered function to even smooth the data.
  - At the moment only LPF (Low Pass Filter), HPF (High Pass Filter), and (BPF )Band Pass Filter. But many other filter are coming.
- Read and write data to serial ports.
- Can plot up to 6 different channels.
- Data logger to save incoming data to a .txt or .csv file.

## File Format
Format file that supported with this application is .csv and .txt. And there has to be a format on those file to be able to read in the app. 
For .csv and .txt format is not that different. On the first row there has to be a title for the data below it. And the maximum column that supported are 6.

So your file must be contain

```
<name A>      <name B>      ...   <name F>
<value A1>    <value B1>    ...   <value F1>
<value A2>    <value B2>    ...   <value F2>
.
.
.
<value An>    <value Bn>    ...   <value Fn>
```

## Plot Serial Data
In order for data to be plotted, incoming data must be send in string format and each data seperated by **semicolon** ```;``` and each line must be seperated by **new line** ```\n```.
Incoming data must be like this

```
var1;var2;var3;var4;var5;var6\n
```

## To Do 
- [ ] Fix the bug when importing data after importing different data.
- [ ] Add new extra menu for each filter for advance setup.
- [ ] Auto add new filtered signal for each new raw data signal. (eg. if raw data signal is 3 then filtered signal must be 3)
- [ ] Use more than 6 plotted graph.
- [ ] Release the app.
