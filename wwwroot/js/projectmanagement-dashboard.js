(function (factory) {
  typeof define === 'function' && define.amd ? define(factory) :
  factory();
})((function () { 'use strict';

  // import * as echarts from 'echarts';
  const { merge } = window._;

  // form config.js
  const echartSetOption = (chart, userOptions, getDefaultOptions) => {
    const themeController = document.body;
    // Merge user options with lodash
    chart.setOption(merge(getDefaultOptions(), userOptions));

    themeController.addEventListener(
      'clickControl',
      ({ detail: { control } }) => {
        if (control === 'phoenixTheme') {
          chart.setOption(window._.merge(getDefaultOptions(), userOptions));
        }
      }
    );
  };
  // -------------------end config.js--------------------

  const resizeEcharts = () => {
    const $echarts = document.querySelectorAll('[data-echart-responsive]');

    if ($echarts.length > 0) {
      $echarts.forEach(item => {
        const echartInstance = echarts.getInstanceByDom(item);
        echartInstance?.resize();
      });
    }
  };

  const navbarVerticalToggle = document.querySelector('.navbar-vertical-toggle');
  navbarVerticalToggle &&
    navbarVerticalToggle.addEventListener('navbar.vertical.toggle', e => {
      return resizeEcharts();
    });

  const tooltipFormatter = (params, dateFormatter = 'MMM DD') => {
    let tooltipItem = ``;
    params.forEach(el => {
      tooltipItem += `<div class='ms-1'>
        <h6 class="text-700"><span class="fas fa-circle me-1 fs--2" style="color:${
          el.borderColor ? el.borderColor : el.color
        }"></span>
          ${el.seriesName} : ${
      typeof el.value === 'object' ? el.value[1] : el.value
    }
        </h6>
      </div>`;
    });
    return `<div>
            <p class='mb-2 text-600'>
              ${
                window.dayjs(params[0].axisValue).isValid()
                  ? window.dayjs(params[0].axisValue).format(dateFormatter)
                  : params[0].axisValue
              }
            </p>
            ${tooltipItem}
          </div>`;
  };

  // dayjs.extend(advancedFormat);

  /* -------------------------------------------------------------------------- */
  /*                             Echarts Total Sales                            */
  /* -------------------------------------------------------------------------- */

  const issuesDiscoveredChartInit = () => {
    const { getColor, getData, resize } = window.phoenix.utils;
    const issuesDiscoveredChartEl = document.querySelector('.echart-issue-chart');

    if (issuesDiscoveredChartEl) {
      const userOptions = getData(issuesDiscoveredChartEl, 'echarts');
      const chart = window.echarts.init(issuesDiscoveredChartEl);

      const getDefaultOptions = () => ({
        color: [
          getColor('info-300'),
          getColor('warning-300'),
          getColor('danger-300'),
          getColor('success-300'),
          getColor('primary')
        ],
        tooltip: {
          trigger: 'item'
        },
        responsive: true,
        maintainAspectRatio: false,

        series: [
          {
            name: 'Tasks assigned to me',
            type: 'pie',
            radius: ['48%', '90%'],
            startAngle: 30,
            avoidLabelOverlap: false,
            // label: {
            //   show: false,
            //   position: 'center'
            // },

            label: {
              show: false,
              position: 'center',
              formatter: '{x|{d}%} \n {y|{b}}',
              rich: {
                x: {
                  fontSize: 31.25,
                  fontWeight: 800,
                  color: getColor('gray-700'),
                  padding: [0, 0, 5, 15]
                },
                y: {
                  fontSize: 12.8,
                  color: getColor('gray-700'),
                  fontWeight: 600
                }
              }
            },
            emphasis: {
              label: {
                show: true
              }
            },
            labelLine: {
              show: false
            },
            data: [
              { value: 78, name: 'Product design' },
              { value: 63, name: 'Development' },
              { value: 56, name: 'QA & Testing' },
              { value: 36, name: 'Customer queries' },
              { value: 24, name: 'R & D' }
            ]
          }
        ],
        grid: {
          bottom: 0,
          top: 0,
          left: 0,
          right: 0,
          containLabel: false
        }
      });

      echartSetOption(chart, userOptions, getDefaultOptions);

      resize(() => {
        chart.resize();
      });
    }
  };

  /* -------------------------------------------------------------------------- */
  /*                             Echarts Total Sales                            */
  /* -------------------------------------------------------------------------- */

  const zeroBurnOutChartInit = () => {
    const { getColor, getData, resize, getPastDates } = window.phoenix.utils;
    const $zeroBurnOutChartEl = document.querySelector(
      '.echart-zero-burnout-chart'
    );

    if ($zeroBurnOutChartEl) {
      const userOptions = getData($zeroBurnOutChartEl, 'echarts');
      const chart = window.echarts.init($zeroBurnOutChartEl);

      const getDefaultOptions = () => ({
        color: [
          getColor('gray-400'),
          getColor('success'),
          getColor('info'),
          getColor('warning')
        ],
        tooltip: {
          trigger: 'axis',
          backgroundColor: getColor('gray-soft'),
          borderColor: getColor('gray-200'),
          formatter: params => tooltipFormatter(params, 'MMM DD, YYYY'),
          axisPointer: {
            shadowStyle: {
              color: 'red'
            }
          }
        },
        legend: {
          bottom: '10',
          data: [
            {
              name: 'Open',
              icon: 'roundRect'
            },
            {
              name: 'Issues found',
              icon: 'roundRect'
            },
            {
              name: 'In Progress',
              icon: 'roundRect'
            }
          ],
          itemWidth: 16,
          itemHeight: 8,
          itemGap: 10,
          inactiveColor: getColor('gray-500'),
          inactiveBorderWidth: 0,
          textStyle: {
            color: getColor('gray-900'),
            fontWeight: 600,
            fontSize: 16,
            fontFamily: 'Nunito Sans'
          }
        },

        // grid: {
        //   left: '0%',
        //   right: '4%',
        //   bottom: '15%',
        //   top: '10%',
        //   containLabel: true,
        //   show: true
        // },

        xAxis: [
          {
            show: true,
            interval: 2,
            axisLine: {
              lineStyle: {
                type: 'solid',
                color: getColor('gray-300')
              }
            },
            axisLabel: {
              color: getColor('gray-900'),
              formatter: data => window.dayjs(data).format('D MMM'),
              interval: 5,
              align: 'left',
              margin: 20,
              fontSize: 12.8
            },
            axisTick: {
              show: true,
              length: 15
              // alignWithLabel: true
            },
            splitLine: {
              interval: 0,
              show: true,
              lineStyle: {
                color: getColor('gray-300'),
                type: 'dashed'
              }
            },
            type: 'category',
            boundaryGap: false,
            data: getPastDates(15)
          },
          {
            show: true,
            interval: 2,
            axisLine: {
              show: false
            },
            axisLabel: {
              show: false
            },
            axisTick: {
              show: false
            },
            splitLine: {
              interval: 1,
              show: true,
              lineStyle: {
                color: getColor('gray-300'),
                type: 'solid'
              }
            },
            boundaryGap: false,
            data: getPastDates(15)
          }
        ],
        yAxis: {
          show: true,
          type: 'value',
          axisLine: {
            lineStyle: {
              type: 'solid',
              color: getColor('gray-300')
            }
          },
          axisLabel: {
            color: getColor('gray-900'),
            margin: 20,
            fontSize: 12.8,
            interval: 0
          },
          splitLine: {
            show: true,
            lineStyle: {
              color: getColor('gray-300'),
              type: 'solid'
            }
          },
          axisTick: {
            show: true,
            length: 15,
            alignWithLabel: true,
            lineStyle: {
              color: getColor('gray-300')
            }
          }
          // data: ['0', '10', '20']
        },
        series: [
          {
            name: 'Estimated',
            type: 'line',
            symbol: 'none',
            data: [20, 17.5, 15, 15, 15, 12.5, 10, 7.5, 5, 2.5, 2.5, 2.5, 0],
            lineStyle: {
              width: 0
            },
            areaStyle: {
              color: getColor('primary-300'),
              opacity: 0.075
            },
            tooltip: {
              show: false
            }
          },
          {
            name: 'Issues found',
            type: 'line',
            symbolSize: 6,
            data: [3, 1, 2, 4, 3, 1]
          },
          {
            name: 'Open',
            type: 'line',
            symbolSize: 6,
            data: [6, 5, 4, 6, 5, 5]
          },
          {
            name: 'In Progress',
            type: 'line',
            symbolSize: 6,
            data: [11, 12, 11, 9, 11, 6]
          },
          {
            name: 'Actual',
            type: 'line',
            symbolSize: 6,
            data: [20, 19, 15, 14, 12, 8],
            lineStyle: {
              type: 'dashed'
            }
          }
        ],
        grid: {
          right: 5,
          left: 0,
          bottom: '15%',
          top: 20,
          containLabel: true
        }
      });

      echartSetOption(chart, userOptions, getDefaultOptions);

      resize(() => {
        chart.resize();
      });
    }
  };

  const { docReady } = window.phoenix.utils;

  docReady(zeroBurnOutChartInit);
  docReady(issuesDiscoveredChartInit);

}));
//# sourceMappingURL=projectmanagement-dashboard.js.map
